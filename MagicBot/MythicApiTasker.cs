using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
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
        private String _pathNewCards;
        private readonly String _pathGetCards = "APIv2/cards/by/spoils";
        private readonly String _pathImages = "card_images";
        private Int32 _numberOfTrysBeforeIgnoringWebSite;
        private String _apiKey;
        #endregion

        #region Constructors
        public MythicApiTasker(String apiUrl, String websiteUrl, String pathNewCards, String apiKey, Int32 numberOfTrysBeforeIgnoringWebSite)
        {
            _apiUrl = apiUrl;
            _websiteUrl = websiteUrl;
            _pathNewCards = pathNewCards;
            _apiKey = apiKey;
            _numberOfTrysBeforeIgnoringWebSite = numberOfTrysBeforeIgnoringWebSite;
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
                    CheckCard(sp, dctWebsiteCards);
                }

            }
        }


        private void CheckCard(SpoilItem sp, Dictionary<String, String> dctWebsiteCards)
        {
            SpoilItem spoil = sp;
            //check if the spoil is in the database
            //if is not in the database AND has NOT been sent
            if (!Database.IsSpoilInDatabase(spoil, true))
            {
                if (dctWebsiteCards.ContainsKey(spoil.CardUrl))
                {
                    String urlCard = dctWebsiteCards.GetValueOrDefault(spoil.CardUrl);
                    spoil.FullUrlWebSite = String.Format("{0}{1}", _websiteUrl, urlCard);
                    spoil = GetAdditionalInfo(spoil);
                }
                else if (dctWebsiteCards.ContainsKey(spoil.CardUrl.Substring(0, spoil.CardUrl.Length - 5) + ".jpg")) //sometimes the api does weird things and returns a random 1 on the end, just test for it also
                {
                    String urlCard = dctWebsiteCards.GetValueOrDefault(spoil.CardUrl.Replace("1", String.Empty));
                    spoil.FullUrlWebSite = String.Format("{0}{1}", _websiteUrl, urlCard);
                    spoil = GetAdditionalInfo(spoil);
                }

                //if the spoil doesn't have any extra info, it will add to the database but it will not send it out yet
                if (!spoil.HasAnyExtraInfo())
                {
                    //if it is below or limit for waiting, we just advance and hope that the next time it is on the website
                    Int32 numberOfTrys = Database.InsertSimpleSpoilAndOrAddCounter(spoil);
                    if (numberOfTrys < _numberOfTrysBeforeIgnoringWebSite)
                    {
                        Program.WriteLine(String.Format("{0} doesn't have enough information, tried {1} times", spoil.CardUrl, numberOfTrys));
                        return;
                    }
                }

                //formats the full path of the image
                String fullUrlImagePath = String.Format("{0}/{1}/{2}/{3}", _apiUrl, _pathImages, spoil.Folder, spoil.CardUrl);
                spoil.ImageUrlWebSite = fullUrlImagePath;

                try
                {
                    spoil.Image = GetImageFromUrl(spoil.ImageUrlWebSite);
                }
                catch (Exception)
                {
                    Program.WriteLine(String.Format("Error getting the image for {0}, will try to replace the number at the end", spoil.CardUrl));
                    spoil.ImageUrlWebSite = spoil.ImageUrlWebSite.Substring(0, spoil.ImageUrlWebSite.Length - 5) + ".jpg";
                    try
                    {
                        spoil.Image = GetImageFromUrl(spoil.ImageUrlWebSite);
                    }
                    catch (Exception)
                    {
                        Program.WriteLine(String.Format("Could not load image for {0}", spoil.CardUrl));
                        return;
                    }
                }

                //adds in the database
                Database.InsertOrUpdateSpoil(spoil);

                //fires the event to do stuffs with the new object
                OnNewItem(spoil);
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
                    spoil.Text = System.Net.WebUtility.HtmlDecode(sb.ToString());
                }
                catch { }
                try { spoil.Flavor = System.Net.WebUtility.HtmlDecode(html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[5]/td[1]/i[1]").LastChild.InnerText.Trim()); } catch { }
                try { spoil.Illustrator = System.Net.WebUtility.HtmlDecode(html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[7]/td[1]/font[1]").LastChild.InnerText.Trim()); } catch { }

                try
                {
                    var nodes = html.DocumentNode.SelectNodes("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[7]/td[2]/font[1]");

                    foreach (var node in nodes)
                    {
                        var powerToughness = node.ChildNodes[2].InnerText.Trim();
                        powerToughness = powerToughness.Replace("\n", String.Empty);
                        if (powerToughness.Contains("/"))
                        {
                            String[] arrPt = powerToughness.Split('/');
                            if (arrPt.Length == 2)
                            {
                                spoil.Power = Int32.Parse(arrPt[0]);
                                spoil.Toughness = Int32.Parse(arrPt[1]);
                                break;
                            }
                        }
                    }


                }
                catch { }
                try
                {
                    //as they layout always changes and we don't know for sure which one will work, we try different ways to get the images
                    try
                    {
                        HtmlNode node = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[1]/img[1]");


                        String outerHtml = node.OuterHtml;
                        String[] tmp = outerHtml.Split("\"");
                        String jpg = tmp[1];
                        String urlSite = spoil.FullUrlWebSite.Substring(0, spoil.FullUrlWebSite.LastIndexOf('/'));
                        if (jpg != spoil.CardUrl)
                        {
                            String fullUrlAdditional = String.Format("{0}/{1}", urlSite, jpg);
                            spoil.AdditionalImageUrlWebSite = fullUrlAdditional;
                            spoil.AdditionalImage = GetImageFromUrl(fullUrlAdditional);
                        }
                    }
                    catch { }

                    if (spoil.AdditionalImage == null)
                    {
                        try
                        {
                            HtmlNodeCollection nodes = html.DocumentNode.SelectNodes("/html/body/center/table[5]/tr[1]/td[1]/img");
                            foreach (HtmlNode htmlNode in nodes)
                            {
                                String img = htmlNode.Attributes["src"].Value.ToString();
                                String urlSite = spoil.FullUrlWebSite.Substring(0, spoil.FullUrlWebSite.LastIndexOf('/'));
                                if (img != spoil.CardUrl &&
                                        !(spoil.CardUrl.StartsWith(img.Replace(".jpg", String.Empty)) ||
                                        img.StartsWith(spoil.CardUrl.Replace(".jpg", String.Empty)))
                                    ) // this if checks for promo so the script doesnt get the same image but for a promo as a additional one)
                                {
                                    String fullUrlAdditional = String.Format("{0}/{1}", urlSite, img);
                                    spoil.AdditionalImageUrlWebSite = fullUrlAdditional;
                                    spoil.AdditionalImage = GetImageFromUrl(fullUrlAdditional);
                                }
                            }
                        }
                        catch { }
                    }

                    
                    if (spoil.AdditionalImage == null)
                    {
                        try
                        {
                            HtmlNodeCollection nodes = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[1]/nobr[1]").SelectNodes("img");
                            foreach (HtmlNode htmlNode in nodes)
                            {
                                String img = htmlNode.Attributes["src"].Value.ToString();
                                String urlSite = spoil.FullUrlWebSite.Substring(0, spoil.FullUrlWebSite.LastIndexOf('/'));
                                if (img != spoil.CardUrl &&
                                        !(spoil.CardUrl.StartsWith(img.Replace(".jpg", String.Empty)) ||
                                        img.StartsWith(spoil.CardUrl.Replace(".jpg", String.Empty)))
                                    ) // this if checks for promo so the script doesnt get the same image but for a promo as a additional one)
                                {
                                    String fullUrlAdditional = String.Format("{0}/{1}", urlSite, img);
                                    spoil.AdditionalImageUrlWebSite = fullUrlAdditional;
                                    spoil.AdditionalImage = GetImageFromUrl(fullUrlAdditional);
                                }
                            }
                        }
                        catch { }
                    }

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
                HtmlDocument doc = htmlWeb.Load(String.Format("{0}{1}", _websiteUrl, _pathNewCards));

                //all the cards are a a href so we get all of that
                HtmlNodeCollection cards = doc.DocumentNode.SelectNodes("//img");
                foreach (HtmlNode card in cards)
                {
                    //also the cards have a special class called 'card', so we use it to get the right ones
                    if (card.Attributes.Contains("src") &&
                    card.Attributes["src"].Value.ToString().Contains("cards") &&
                    card.Attributes["src"].Value.ToString().EndsWith(".jpg"))
                    {
                        //we get the information and put on a dictionary
                        //we do this way because it is easier to see if you can get aditional info later

                        HtmlAttribute linkToCard = card.ParentNode.Attributes["href"];
                        String imageJpg = card.Attributes["src"].Value.Split('/').Last();
                        if (!String.IsNullOrEmpty(imageJpg))
                        {
                            if (!dct.ContainsKey(imageJpg))
                            {
                                dct.Add(imageJpg, linkToCard.Value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.WriteLine("Error crawling the main page ");
                Program.WriteLine(ex.Message);
                Program.WriteLine(ex.StackTrace);
            }
            return dct;
        }

        #endregion
    }
}
