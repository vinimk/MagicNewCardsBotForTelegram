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
        async public static Task Main(string[] args)
        {

            try
            {
                //first initialize our internals
                Init();

                //first we update the list of chats
                Program.WriteLine("Updating telegram chat list");
                await _telegramController.InitialUpdate();
                _telegramController.HookUpdateEvent();
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
                    await _mythicApiTasker.GetNewCards();

                    //we wait for a while before executing again, this interval be changed in the appsettings.json file
                    Program.WriteLine(String.Format("Going to sleep for {0} ms.", _timeInternalMS));
                    await Task.Delay(_timeInternalMS);
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
                    await Task.Delay(_timeInternalMS);
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

            // _mtgSalvationTasker = new MTGSalvationTasker();
            // _mtgSalvationTasker.eventNewcard += EventNewcard;

            _mythicApiTasker = new MythicApiTasker(config["MythicWebsiteUrl"], config["MythicWebsitePathNewCards"]);
            _mythicApiTasker.eventNewcard += EventNewcard;

            //_mtgVisualApiTasker = new MTGVisualTasker();
            //_mtgVisualApiTasker.eventNewcard += EventNewcard;

            // _scryfallApiTasker = new ScryfallApiTasker();
            // _scryfallApiTasker.eventNewcard += EventNewcard;

            _telegramController = new TelegramController(config["TelegramBotApiKey"]);

            _twitterController = new TwitterController(config["TwitterConsumerKey"], config["TwitterConsumerSecret"], config["TwitterAcessToken"], config["TwitterAcessTokenSecret"]);

        }

        #endregion

        #region Events Handlers

        private static void EventNewcard(object sender, Card newItem)
        {
            if (newItem.ImageUrl != null)
            {
                Program.WriteLine(String.Format("Sending new card {0} to everyone", newItem.Name));
                try
                {
                    _telegramController.SendImageToAll(newItem).Wait();
                }
                catch (Exception ex)
                {
                    Database.InsertLog("Telegram send images to all", newItem.Name, ex.ToString()).Wait();
                    Program.WriteLine(String.Format("Failed to send to telegram spoil {0}", newItem.Name));
                    Program.WriteLine(ex.Message);
                }

                Program.WriteLine(String.Format("Tweeting new card {0}", newItem.Name));
                try
                {
                    _twitterController.PublishNewImage(newItem).Wait();
                    Database.UpdateIsSent(newItem, true).Wait();
                }
                catch (Exception ex)
                {
                    Database.InsertLog("Twitter send image", newItem.Name, ex.ToString()).Wait();
                    Program.WriteLine(String.Format("Failed to send to twitter spoil {0}", newItem.Name));
                    Program.WriteLine(ex.Message);
                }
            }
        }
        #endregion

        #region Helper Functions
        public static Stream GetImageFromUrl(String url)
        {
            //do a webrequest to get the image
            HttpWebRequest imageRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse imageResponse = (HttpWebResponse)imageRequest.GetResponse();
            Stream imageStream = imageResponse.GetResponseStream();

            return imageStream;
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static void WriteLine(String message)
        {
            Console.WriteLine(String.Format("{0}-{1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), message));
        }
        #endregion

    }

}