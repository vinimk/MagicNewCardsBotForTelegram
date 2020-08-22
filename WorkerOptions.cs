using System;
using System.Collections.Generic;
using System.Text;

namespace MagicNewCardsBot
{
    public class WorkerOptions
    {
        public int TimeExecuteIntervalInMs { get; set; }
        public int NumberOfTrysBeforeIgnoringWebSite { get; set; }
        public string ConnectionStringMySQL { get; set; }
        public string MythicWebsiteUrl { get; set; }
        public string MythicWebsitePathNewCards { get; set; }
        public string MythicApiUrl { get; set; }
        public string MythicApiKey { get; set; }
        public string TelegramBotApiKey { get; set; }
        public string TwitterConsumerKey { get; set; }
        public string TwitterConsumerSecret { get; set; }
        public string TwitterAcessToken { get; set; }
        public string TwitterAcessTokenSecret { get; set; }
        public long TelegramDebugUserID { get; set; }
    }
}
