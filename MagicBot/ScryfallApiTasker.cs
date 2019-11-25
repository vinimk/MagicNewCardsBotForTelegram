using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MagicBot
{
    public class ScryfallApiTasker
    {
        internal readonly String _apiUrl = "https://api.scryfall.com/cards?page=0";
        public ScryfallApiTasker()
        { }

        #region Public Methods
        async public Task GetNewCards()
        {
            await CheckNewCards();
        }
        #endregion

        #region Private Methods
        async private Task CheckNewCards()
        {
            String jsonMsg = await GetFromAPI();

            if (!String.IsNullOrEmpty(jsonMsg))
            {
                //deserialization of the objects
                ScryfallApiResponse response = JsonConvert.DeserializeObject<ScryfallApiResponse>(jsonMsg);

                foreach (ScryfallCard card in response.Data)
                {
                    await CheckCard(card);
                }

            }
        }

        async private Task CheckCard(ScryfallCard card)
        {
            //check if the card is in the database and has NOT been sent
            var isInDb = await Database.IsCardInDatabase(card, true);
            if (isInDb == false)
            {
                //if the card is a transform card, it needs to load both sides differently 
                if (card.layout == "transform" ||
                    card.layout == "double_faced_token")
                {
                    Boolean isMainFace = true;
                    foreach (var face in card.card_faces)
                    {
                        String name = null, type_line = null, oracle_text = null, mana_cost = null, power = null, toughness = null, flavor_text = null, image_url = null;

                        if (face["name"] != null)
                        {
                            name = face["name"].ToString();
                        }

                        if (face["type_line"] != null)
                        {
                            type_line = face["type_line"].ToString();
                        }

                        if (face["oracle_text"] != null)
                        {
                            oracle_text = face["oracle_text"].ToString();
                        }

                        if (face["mana_cost"] != null)
                        {
                            mana_cost = face["mana_cost"].ToString();
                        }

                        if (face["power"] != null)
                        {
                            power = face["power"].ToString();
                        }

                        if (face["toughness"] != null)
                        {
                            toughness = face["toughness"].ToString();
                        }

                        if (face["flavor_text"] != null)
                        {
                            flavor_text = face["flavor_text"].ToString();
                        }

                        if (face["image_uris"] != null)
                        {
                            image_url = face["image_uris"]["png"].ToString();
                        }

                        if (isMainFace)
                        {
                            card.Name = name;
                            card.Type = type_line;
                            card.Text = oracle_text;
                            card.ManaCost = mana_cost;
                            card.Power = power;
                            card.Toughness = toughness;
                            card.Flavor = flavor_text;
                            card.ImageUrl = image_url;

                            isMainFace = false;
                        }
                        else
                        {
                            ScryfallCard extraCard = new ScryfallCard
                            {
                                Name = name,
                                Type = type_line,
                                Text = oracle_text,
                                ManaCost = mana_cost,
                                Power = power,
                                Toughness = toughness,
                                Flavor = flavor_text,
                                ImageUrl = image_url
                            };

                            card.ExtraSides.Add(extraCard);
                        }
                    }
                }
                else
                {
                    card.ImageUrl = card.image_uris["png"].ToString();
                }

                //adds in the database
                await Database.InsertScryfallCard(card);
                //fires the event to do stuff with it
                OnNewCard(card);
            }
        }

        private async Task<String> GetFromAPI()
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
                await Database.InsertLog("Scryfall", String.Empty, ex.ToString());
                throw new Exception("API Problem", ex);
            }
        }

        #endregion

        #region Events
        public delegate void NewCard(object sender, Card newItem);
        public event NewCard EventNewcard;
        protected virtual void OnNewCard(Card args)
        {
            EventNewcard?.Invoke(this, args);
        }
        #endregion
    }
}