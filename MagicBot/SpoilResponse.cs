using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MagicBot
{
    public class SpoilResponse
    {
        [JsonProperty("status")]
        public String Status { get; set; }
        [JsonProperty("item")]
        public List<SpoilItem> Items { get; set; }
        [JsonProperty("logo")]
        public String Logo { get; set; }
    }
}
