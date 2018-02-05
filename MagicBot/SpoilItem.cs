using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Net;
using System.Text;

namespace MagicBot
{
    public class SpoilItem : Card
    {
        public Int64 SpoilItemId
        {
            get;
            set;
        }

        public SpoilItem(): base()
        { }
    }
}