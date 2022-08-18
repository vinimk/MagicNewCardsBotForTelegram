using Newtonsoft.Json.Linq;

namespace MagicNewCardsBot.StorageClasses
{
    public class ScryfallCard : Card
    {
        public string id
        {
            get;
            set;
        }
        public string scryfall_uri
        {
            get;
            set;
        }
        public string layout
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