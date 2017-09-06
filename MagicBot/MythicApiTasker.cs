using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using ImageSharp;
using System.Net;
using System.IO;
using HtmlAgilityPack;

namespace MagicBot
{
    public class MythicApiTasker
    {
        #region Definitions
        private String _apiUrl;
        private String _websiteUrl;
        private readonly String _pathGetCards = "APIv2/cards/by/spoils";
        private readonly String _pathImages = "card_images";
        private String _apiKey;
        #endregion

        #region Constructors
        public MythicApiTasker(String apiUrl, String websiteUrl, String apiKey)
        {
            _apiUrl = apiUrl;
            _websiteUrl = websiteUrl;
            _apiKey = apiKey;
        }
        #endregion

        #region Events
        public delegate void NewItem(object sender, SpoilItem newItem);
        public event NewItem New;
        protected virtual void OnNewItem(SpoilItem args)
        {
            if (New != null)
                New(this, args);
        }
        #endregion

        #region Public Methods
        public void GetNewCards()
        {
            CheckNewCards();
        }
        #endregion

        #region Private Methods
        private void CheckNewCards()
        {
            String jsonMsg = GetFromAPI();

            //find better wait to remove the ()
            //it might be because we are getting info from the API in the wrong way
            jsonMsg = jsonMsg.Replace("(", "");
            jsonMsg = jsonMsg.Replace(")", "");


            if (!String.IsNullOrEmpty(jsonMsg))
            {
                //deserialization of the objects
                SpoilResponse response = JsonConvert.DeserializeObject<SpoilResponse>(jsonMsg);

                //get the aditional infos from the website
                Dictionary<String, String> dctWebsiteCards = GetAvaliableCardsInWebSite();

                foreach (SpoilItem sp in response.Items)
                {
                    SpoilItem spoil = sp;
                    //check if the spoil is in the database
                    Task<Boolean> taskDb = Database.IsSpoilInDatabase(spoil);
                    taskDb.Wait();
                    //if is not in the database
                    if (!taskDb.Result)
                    {
                        if (dctWebsiteCards.ContainsKey(spoil.CardUrl))
                        {
                            String urlCard = dctWebsiteCards.GetValueOrDefault(spoil.CardUrl);
                            spoil.FullUrlWebSite = String.Format("{0}/{1}", _websiteUrl, urlCard);
                            spoil = GetAdditionalInfo(spoil);
                        }

                        //formats the full path of the image
                        String fullUrlImagePath = String.Format("{0}/{1}/{2}/{3}", _apiUrl, _pathImages, spoil.Folder, spoil.CardUrl);
                        spoil.ImageUrlWebSite = fullUrlImagePath;

                        //adds in the database
                        //does it async and doesn't need to wait because we don't need the 
                        Database.InsertSpoil(spoil).Wait();

                        try
                        {
                            spoil.Image = GetImageFromUrl(fullUrlImagePath);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error getting the image", ex);
                        }

                        //fires the event to do stuffs with the new object
                        OnNewItem(spoil);
                    }

                }
            }
        }

        private Image<Rgba32> GetImageFromUrl(String url)
        {
            //do a webrequest to get the image
            HttpWebRequest imageRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse imageResponse = (HttpWebResponse)imageRequest.GetResponse();
            Stream imageStream = imageResponse.GetResponseStream();

            //loads the stream into an image object
            Image<Rgba32> retImage = Image.Load<Rgba32>(imageStream);

            //closes the webresponse and the stream
            imageStream.Close();
            imageResponse.Close();
            return retImage;
        }

        private String GetFromAPI()
        {
            try
            {
                //creates an http client and makes the request for all the cards
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(String.Format("{0}/{1}?key={2}", _apiUrl, _pathGetCards, _apiKey));
                httpWebRequest.ContentType = "application/json; charset=utf-8";
                httpWebRequest.Accept = "application/json";
                httpWebRequest.Method = WebRequestMethods.Http.Get;
                using (var streamReader = new StreamReader(((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream()))
                {
                    return streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("API Problem", ex);
            }
        }

        private SpoilItem GetAdditionalInfo(SpoilItem spoil)
        {
            //we do all of this in empty try catches because it is not mandatory information
            try
            {
                //crawl the webpage to get this information
                HtmlWeb htmlWeb = new HtmlWeb();
                HtmlDocument html = htmlWeb.Load(spoil.FullUrlWebSite);
                try { spoil.Name = html.DocumentNode.SelectSingleNode("html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[1]/td[1]/font[1]").LastChild.InnerText.Trim(); } catch { }
                try { spoil.ManaCost = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[2]/td[1]").LastChild.InnerText.Trim(); } catch { }
                try { spoil.Type = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[3]/td[1]").LastChild.InnerText.Trim(); } catch { }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    var nodes = html.DocumentNode.SelectNodes("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[4]/td[1]");
                    foreach (var node in nodes)
                    {
                        String txt = node.InnerText;
                        txt = txt.Replace("\n\n", "\n");
                        txt = txt.Replace(@"<!--CARD TEXT-->", String.Empty);
                        sb.Append(txt.Trim());
                    }
                    spoil.Text = sb.ToString();
                }
                catch { }
                try { spoil.Flavor = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[5]/td[1]/i[1]").LastChild.InnerText.Trim(); } catch { }
                try { spoil.Illustrator = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[7]/td[1]/font[1]").LastChild.InnerText.Trim(); } catch { }

                try
                {
                    String powerToughness = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[7]/td[2]/font[1]").ChildNodes[2].InnerText.Trim();

                    if (powerToughness.Contains("/"))
                    {
                        String[] arrPt = powerToughness.Split('/');
                        if (arrPt.Length == 2)
                        {
                            spoil.Power = Int32.Parse(arrPt[0]);
                            spoil.Toughness = Int32.Parse(arrPt[1]);
                        }
                    }
                }
                catch { }
                try
                {
                    String outerHtml = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[1]/img[1]").OuterHtml;
                    String[] tmp = outerHtml.Split("\"");
                    String jpg = tmp[1];
                    String urlSite = spoil.FullUrlWebSite.Substring(0, spoil.FullUrlWebSite.LastIndexOf('/'));
                    String fullUrlAdditional = String.Format("{0}/{1}", urlSite, jpg);
                    spoil.AdditionalImageUrlWebSite = fullUrlAdditional;
                    spoil.AdditionalImage = GetImageFromUrl(fullUrlAdditional);

                }
                catch { }
            }
            catch { }
            return spoil;
        }

        private Dictionary<String, String> GetAvaliableCardsInWebSite()
        {
            Dictionary<String, String> dct = new Dictionary<String, String>();
            try
            {
                //loads the website
                HtmlWeb htmlWeb = new HtmlWeb();
                HtmlDocument doc = htmlWeb.Load(_websiteUrl);

                //all the cards are a a href so we get all of that
                HtmlNodeCollection cards = doc.DocumentNode.SelectNodes("//a");
                foreach (HtmlNode card in cards)
                {
                    //also the cards have a special class called 'card', so we use it to get the right ones
                    if (card.Attributes.Contains("class") && card.Attributes["class"].Value == "card")
                    {
                        //we get the information and put on a dictionary
                        //we do this way because it is easier to see if you can get aditional info later
                        HtmlAttribute att = card.Attributes["href"];
                        String imageJpg = att.Value.Split('/').Last().Replace(".html", ".jpg");
                        if (!String.IsNullOrEmpty(imageJpg))
                        {
                            dct.Add(imageJpg, att.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error crawling the main page ");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return dct;
        }

        #endregion
    }
}
