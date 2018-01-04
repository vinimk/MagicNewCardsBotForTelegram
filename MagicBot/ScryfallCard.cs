using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace MagicBot
{
    public class ScryfallCard : Card
    {
        public String id { get; set; }
        public String scryfall_uri { get; set; }
        public String layout { get; set; }
        public JToken image_uris { get; set; }
        public JToken card_faces { get; set; }

        public ScryfallCard() : base()
        {
        }
    }
}
