using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.IO;
using System.Net;

namespace MagicBot
{
    public class SpoilItem : Card
    {
        public Int64 SpoilItemId { get; set; }

        public SpoilItem() : base()
        {
        }
    }
}
