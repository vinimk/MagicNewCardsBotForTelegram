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
using ImageSharp;
using System.Collections.Generic;

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
                Program.WriteLine("Updating telegram chat list");
                _telegramController.InitialUpdate();
                _telegramController.HookUpdateEvent();
            }
            catch (Exception ex)
            {
                Program.WriteLine(ex.ToString());
            }

            //the software will always be on a while true loop
            while (true)
            {
                try
                {
                    //we get the new cards
                    //note that since we have a event handler for new cards, the event will be fired if a new card is found
                    Program.WriteLine("Getting new cards");
                    _mythicApiTasker.GetNewCards();

                    //we wait for a while before executing again, this interval be changed in the appsettings.json file
                    Program.WriteLine(String.Format("Going to sleep for {0} ms. I might still be doing work on the background", _timeInternalMS));
                    Thread.Sleep(_timeInternalMS);
                }
                catch (Exception ex)
                {
                    Program.WriteLine(ex.ToString());
                }
            }
        }

        #region Definitions
        private static MythicApiTasker _mythicApiTasker;
        private static TelegramController _telegramController;
        private static TwitterController _twitterController;
        private static int _timeInternalMS;
        #endregion

        #region Init configs
        private static void Init()
        {
            Program.WriteLine("Initializing");

            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            var config = builder.Build();

            _timeInternalMS = Int32.Parse(config["TimeExecuteIntervalInMs"]);
            Database.SetConnectionString(config["ConnectionStringMySQL"]);

            _mythicApiTasker = new MythicApiTasker(config["MythicApiUrl"], config["MythicWebsiteUrl"], config["MythicWebsitePathNewCards"], config["MythicApiKey"], Int32.Parse(config["NumberOfTrysBeforeIgnoringWebSite"]));
            _mythicApiTasker.New += MythicApiTasker_New;

            _telegramController = new TelegramController(config["TelegramBotApiKey"]);

            _twitterController = new TwitterController(config["TwitterConsumerKey"], config["TwitterConsumerSecret"], config["TwitterAcessToken"], config["TwitterAcessTokenSecret"]);

        }
        #endregion

        #region Events Handlers
        private static void MythicApiTasker_New(object sender, SpoilItem newItem)
        {
            if (newItem.Image != null)
            {
                Program.WriteLine(String.Format("Sending new card {0} from folder {1} to everyone", newItem.CardUrl, newItem.Folder));
                try
                {
                    Task.Run(() => _telegramController.SendImageToAll(newItem));
                }
                catch (Exception ex)
                {
                    Program.WriteLine(String.Format("Failed to send to telegram spoil {0}", newItem.CardUrl));
                    Program.WriteLine(ex.Message);
                }

                Program.WriteLine(String.Format("Tweeting new card {0} from folder {1}", newItem.CardUrl, newItem.Folder));
                try
                {
                    Task.Run(() =>_twitterController.PublishNewImage(newItem));
                }
                catch (Exception ex)
                {
                    Program.WriteLine(String.Format("Failed to send to twitter spoil {0}", newItem.CardUrl));
                    Program.WriteLine(ex.Message);
                }

                Database.UpdateIsSent(newItem, true);
            }
        }
        #endregion

        public static void WriteLine(String message)
        {
            Console.WriteLine(String.Format("{0}-{1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), message));
        }
    }
}
