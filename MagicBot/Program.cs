using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Timers;
using System.Threading;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MagicBot
{
    public class Program
    {

        #region Definitions
        private static MythicApiTasker _mythicApiTasker;
        private static TelegramController _telegramController;
        private static int _timeInternalMS;
        #endregion
        static void Main(string[] args)
        {

            Init();
            while (true)
            {
                try
                {
                    Console.WriteLine("Updating telegram chat list");
                    _telegramController.Update();

                    Console.WriteLine("Getting new cards");
                    _mythicApiTasker.QueryApi();


                    Console.WriteLine(String.Format("Going to sleep for {0} ms", _timeInternalMS));
                    Thread.Sleep(_timeInternalMS);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

        }

        private static void Init()
        {
            Console.WriteLine("Initializing");

            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            var config = builder.Build();

            _timeInternalMS = Int32.Parse(config["TimeExecuteIntervalInMs"]);

            _mythicApiTasker = new MythicApiTasker(config["MythicApiUrl"].ToString(), config["MythicApiKey"].ToString());
            _mythicApiTasker.New += MythicApiTasker_New;

            _telegramController = new TelegramController(config["TelegramBotApiKey"].ToString());
        }

        private static void MythicApiTasker_New(object sender, SpoilItem newItem)
        {
            if (newItem.Image != null)
            {
                Console.WriteLine(String.Format("Sending new card {0} from folder {1} to everyone", newItem.CardUrl, newItem.Folder));

                _telegramController.SendImageToAll(newItem.Image, newItem.CardUrl);
            }
        }
    }
}
