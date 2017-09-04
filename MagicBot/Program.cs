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
        static void Main(string[] args)
        {

            try
            {
                //first initialize our internals
                Init();

                //first we update the list of chats
                Console.WriteLine("Updating telegram chat list");
                _telegramController.InitialUpdate();
                _telegramController.HookUpdateEvent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            //the software will always be on a while true loop
            while (true)
            {
                try
                {
                    //first we update the internal list for telegram
                    _telegramController.UpdateChatInternalList();
                    //we get the new cards
                    //note that since we have a event handler for new cards, the event will be fired if a new card is found
                    Console.WriteLine("Getting new cards");
                    _mythicApiTasker.GetNewCards();

                    //we wait for a while before executing again, this interval be changed in the appsettings.json file
                    Console.WriteLine(String.Format("Going to sleep for {0} ms", _timeInternalMS));
                    Thread.Sleep(_timeInternalMS);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

        }

        #region Definitions
        private static MythicApiTasker _mythicApiTasker;
        private static TelegramController _telegramController;
        private static Database _db;
        private static int _timeInternalMS;
        #endregion

        #region Init configs
        private static void Init()
        {
            Console.WriteLine("Initializing");

            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            var config = builder.Build();

            _timeInternalMS = Int32.Parse(config["TimeExecuteIntervalInMs"]);

            _db = new Database(config["ConnectionStringMySQL"]);

            _mythicApiTasker = new MythicApiTasker(config["MythicApiUrl"].ToString(), config["MythicApiKey"].ToString(), _db);
            _mythicApiTasker.New += MythicApiTasker_New;

            _telegramController = new TelegramController(config["TelegramBotApiKey"].ToString(), _db);
        }
        #endregion

        #region Events Handlers
        private static void MythicApiTasker_New(object sender, SpoilItem newItem)
        {
            if (newItem.Image != null)
            {
                Console.WriteLine(String.Format("Sending new card {0} from folder {1} to everyone", newItem.CardUrl, newItem.Folder));

                _telegramController.SendImageToAll(newItem.Image, newItem.CardUrl);
            }
        }
        #endregion
    }
}
