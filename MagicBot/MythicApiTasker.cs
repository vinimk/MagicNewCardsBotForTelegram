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
        #region Definitions
        private String _apiUrl;
        private readonly String _pathGetCards = "APIv2/cards/by/spoils";
        private readonly String _pathImages = "card_images";
        private String _apiKey;
        private Database _db;
        #endregion

        #region Constructors
        public MythicApiTasker(String apiUrl, String apiKey, Database db)
        {
            _db = db;
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


                foreach (SpoilItem spoil in response.Items)
                {
                    //get the spoils from the database
                    Task<List<SpoilItem>> taskDb = _db.GetAllSpoils();
                    taskDb.Wait();
                    List<SpoilItem> lstSpoils = taskDb.Result;

                    //see if the item is already on the database
                    var data = lstSpoils.Where(x =>
                                               x.CardUrl.Equals(spoil.CardUrl) &&
                                               x.Folder.Equals(spoil.Folder) &&
                                               x.Date.Equals(spoil.Date)
                                              );

                    //if cant find in the database
                    if (data.Count() == 0)
                    {
                        //adds in the database
                        //does it async and doesn't need to wait because we don't need the 
                        _db.InsertSpoil(spoil).Wait();

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
            }
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
        #endregion
    }
}
