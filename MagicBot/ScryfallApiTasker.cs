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
    public class ScryfallApiTasker
    {
        internal readonly String  _apiUrl = "https://api.scryfall.com/cards?page=0";
        public ScryfallApiTasker()
        {
        }

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

            if (!String.IsNullOrEmpty(jsonMsg))
            {
                //deserialization of the objects
                ScryfallApiResponse response = JsonConvert.DeserializeObject<ScryfallApiResponse>(jsonMsg);

                foreach (ScryfallCard card in response.data)
                {
                    CheckCard(card);
                }

            }
        }

        private void CheckCard(ScryfallCard card)
        {
            //check if the card is in the database and has NOT been sent
            if (!Database.IsCardInDatabase(card, true))
            {
                //if the card is a transform card, it needs to load both sides differently 
                if(card.layout == "transform")
                {
                    
                }
                else
                {
card.Image = Program.GetImageFromUrl(card.image_uris["png"].ToString());
                }
                
                // try
                // {
                //     card.Image = Program.GetImageFromUrl(card.image_uris);
                // }
                // catch (Exception)
                // {
                //     Program.WriteLine(String.Format("Error getting the image for {0}, will try to replace the number at the end", spoil.CardUrl));
                //     spoil.ImageUrlWebSite = spoil.ImageUrlWebSite.Substring(0, spoil.ImageUrlWebSite.Length - 5) + ".jpg";
                //     try
                //     {
                //         spoil.Image = GetImageFromUrl(spoil.ImageUrlWebSite);
                //     }
                //     catch (Exception)
                //     {
                //         Program.WriteLine(String.Format("Could not load image for {0}", spoil.CardUrl));
                //         return;
                //     }
                // }

                // //adds in the database
                // Database.InsertOrUpdateSpoil(spoil);

                // //fires the event to do stuffs with the new object
                // OnNewItem(spoil);
            }
        }

        private String GetFromAPI()
        {
            try
            {
                //creates an http client and makes the request for all the cards
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(_apiUrl);
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
                Database.InsertLog("Scryfall", String.Empty, ex.ToString());
                throw new Exception("API Problem", ex);
            }
        }

#endregion

        #region Events
        public delegate void NewCard(object sender, ScryfallCard newItem);
        public event NewCard eventNewcard;
        protected virtual void OnNewCard(ScryfallCard args)
        {
            if (eventNewcard != null)
                eventNewcard(this, args);
        }
        #endregion
    }
}
