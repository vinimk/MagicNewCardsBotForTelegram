using Newtonsoft.Json.Linq;
using System;

namespace MagicNewCardsBot
{
    public class ScryfallCard : Card
    {
        public String id
        {
            get;
            set;
        }
        public String scryfall_uri
        {
            get;
            set;
        }
        public String layout
        {
            get;
            set;
        }
        public JToken image_uris
        {
            get;
            set;
        }
        public JToken card_faces
        {
            get;
            set;
        }

        public ScryfallCard() : base()
        { }
    }
}