using System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Telegram.Bot.Types;

namespace MagicBot
{
    public class Program
    {
        async public static Task Main()
        {

            try
            {
                //first initialize our internals
                Init();

                //first we update the list of chats
                Program.WriteLine("Updating telegram chat list");
                await TelegramController.InitialUpdate();
                TelegramController.HookUpdateEvent();
            }
            catch (Exception ex)
            {
                await Database.InsertLog("Updating telegram chat list", String.Empty, ex.ToString());
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
                    await MythicApiTasker.GetNewCards();

                    //we wait for a while before executing again, this interval be changed in the appsettings.json file
                    Program.WriteLine(String.Format("Going to sleep for {0} ms.", TimeInternalMS));
                    await Task.Delay(TimeInternalMS);
                }
                catch (Exception ex)
                {
                    try
                    {
                        await Database.InsertLog("Getting new cards", String.Empty, ex.ToString());
                        Program.WriteLine(ex.ToString());
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine("Exception in catch, sad");
                        Console.WriteLine(ex2.ToString());
                    }
                    await Task.Delay(TimeInternalMS);
                }
            }
        }

        #region Definitions
        //private static MTGVisualTasker _mtgVisualApiTasker;
        private static MythicApiTasker _mythicApiTasker;
        //private static MTGSalvationTasker _mtgSalvationTasker;
        //private static ScryfallApiTasker _scryfallApiTasker;
        private static TelegramController _telegramController;
        private static TwitterController _twitterController;
        private static int _timeInternalMS;

        public static int TimeInternalMS { get => _timeInternalMS; set => _timeInternalMS = value; }
        public static TwitterController TwitterController { get => _twitterController; set => _twitterController = value; }
        public static TelegramController TelegramController { get => _telegramController; set => _telegramController = value; }
        public static MythicApiTasker MythicApiTasker { get => _mythicApiTasker; set => _mythicApiTasker = value; }
        #endregion

        #region Init configs
        private static void Init()
        {
            Program.WriteLine("Initializing");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = builder.Build();

            TimeInternalMS = Int32.Parse(config["TimeExecuteIntervalInMs"]);

            Database.SetConnectionString(config["ConnectionStringMySQL"]);

            // _mtgSalvationTasker = new MTGSalvationTasker();
            // _mtgSalvationTasker.eventNewcard += EventNewcard;

            MythicApiTasker = new MythicApiTasker(config["MythicWebsiteUrl"], config["MythicWebsitePathNewCards"]);
            MythicApiTasker.EventNewcard += async (s, e) =>
            {
                await EventNewcardAsync(e);
            };

            //_mtgVisualApiTasker = new MTGVisualTasker();
            //_mtgVisualApiTasker.eventNewcard += EventNewcard;

            // _scryfallApiTasker = new ScryfallApiTasker();
            // _scryfallApiTasker.eventNewcard += EventNewcard;

            TelegramController = new TelegramController(config["TelegramBotApiKey"]);

            TwitterController = new TwitterController(config["TwitterConsumerKey"], config["TwitterConsumerSecret"], config["TwitterAcessToken"], config["TwitterAcessTokenSecret"]);

        }

        #endregion

        #region Events Handlers

        async private static Task EventNewcardAsync(Card newItem)
        {
            if (newItem.ImageUrl != null)
            {
                Program.WriteLine(String.Format("Sending new card {0} to everyone", newItem.Name));
                await Database.UpdateIsSent(newItem, true);
                try
                {
                    await TelegramController.SendImageToAll(newItem);
                }
                catch (Exception ex)
                {
                    await Database.InsertLog("Telegram send images to all", newItem.Name, ex.ToString());
                    Program.WriteLine(String.Format("Failed to send to telegram spoil {0}", newItem.Name));
                    Program.WriteLine(ex.Message);
                }

                Program.WriteLine(String.Format("Tweeting new card {0}", newItem.Name));
                try
                {
                    //await TwitterController.PublishNewImage(newItem);
                }
                catch (Exception ex)
                {
                    await Database.InsertLog("Twitter send image", newItem.Name, ex.ToString());
                    Program.WriteLine(String.Format("Failed to send to twitter spoil {0}", newItem.Name));
                    Program.WriteLine(ex.Message);
                }
            }
        }
        #endregion

        #region Helper Functions
        async public static Task<Stream> GetImageFromUrlStreamAsync(String url)
        {
            //do a webrequest to get the image
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStreamAsync(url);
            }
        }

        async public static Task<byte[]> GetImageFromUrlByteArrayAsync(String url)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetByteArrayAsync(url);
            }
        }

        public static void WriteLine(String message)
        {
            Console.WriteLine(String.Format("{0}-{1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), message));
        }
        #endregion

    }

}