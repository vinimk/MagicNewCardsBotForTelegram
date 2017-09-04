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

namespace MagicBot
{
    public class MythicApiTasker
    {
        #region Attributes

        private static string _apiUrl;
        private static readonly string _pathGetCards = "APIv2/cards/by/spoils";
        private static readonly string _pathImages = "card_images";
        private static string _apiKey;

        private SpoilDbContext db;
        #endregion

        #region Constructors
        public MythicApiTasker(String apiUrl, String apiKey)
        {
            db = new SpoilDbContext();
            _apiKey = apiKey;
            _apiUrl = apiUrl;
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

        public void QueryApi()
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


                foreach (SpoilItem spoil in response.Items)
                {
                    //see if the item is already on the database
                    var data = db.Spoils.Where(x =>
                                               x.CardUrl.Equals(spoil.CardUrl) &&
                                               x.Folder.Equals(spoil.Folder) &&
                                               x.Date.Equals(spoil.Date)
                                              );

                    //if cant find in the database
                    if (data.Count() == 0)
                    {
                        //adds in the database
                        db.Spoils.Add(spoil);

                        try
                        {
                            //formats the full path of the image
                            String fullUrlImagePath = String.Format("{0}/{1}/{2}/{3}", _apiUrl, _pathImages, spoil.Folder, spoil.CardUrl);

                            //do a webrequest to get the image
                            HttpWebRequest imageRequest = (HttpWebRequest)WebRequest.Create(fullUrlImagePath);
                            HttpWebResponse imageResponse = (HttpWebResponse)imageRequest.GetResponse();
                            Stream imageStream = imageResponse.GetResponseStream();

                            //loads the stream into an image object
                            spoil.Image = Image.Load<Rgba32>(imageStream);

                            //closes the webresponse and the stream
                            imageStream.Close();
                            imageResponse.Close();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error getting the image", ex);
                        }

                        //fires the event to do stuffs with the new object
                        OnNewItem(spoil);
                    }
                }

                //save the changes to the database
                db.SaveChanges(true);
            }
        }
    }
}
