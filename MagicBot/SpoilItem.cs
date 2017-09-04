using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.IO;
using ImageSharp;

namespace MagicBot
{
    public class SpoilItem
    {
        public long SpoilItemId { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("folder")]
        public string Folder { get; set; }
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("cardUrl")]
        public string CardUrl { get; set; }
        [NotMapped]
        public Image<Rgba32> Image { get; set; }
    }
}
