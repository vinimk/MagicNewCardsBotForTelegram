using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Net;
using System.Text;

namespace MagicBot
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

        public ScryfallCard(): base()
        { }
    }
}