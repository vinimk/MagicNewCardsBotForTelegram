using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace MagicBot
{
    public class ScryfallApiResponse
    {
        public List<ScryfallCard> data
        {
            get;
            set;
        }
    }
}