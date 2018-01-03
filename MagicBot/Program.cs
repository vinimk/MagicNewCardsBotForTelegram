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
using System.Collections.Generic;
using System.Net;

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
                Database.InsertLog("Updating telegram chat list", String.Empty, ex.ToString());
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
                    _scryfallApiTasker.GetNewCards();

                    //we wait for a while before executing again, this interval be changed in the appsettings.json file
                    Program.WriteLine(String.Format("Going to sleep for {0} ms.", _timeInternalMS));
                    Thread.Sleep(_timeInternalMS);
                }
                catch (Exception ex)
                {
                    Database.InsertLog("Getting new cards", String.Empty, ex.ToString());
                    Program.WriteLine(ex.ToString());
                }
            }
        }

        #region Definitions
        private static ScryfallApiTasker _scryfallApiTasker;
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
            
            _scryfallApiTasker = new ScryfallApiTasker();
            _scryfallApiTasker.eventNewcard += _scryfallApiTasker_eventNewcard;

            _telegramController = new TelegramController(config["TelegramBotApiKey"]);

            _twitterController = new TwitterController(config["TwitterConsumerKey"], config["TwitterConsumerSecret"], config["TwitterAcessToken"], config["TwitterAcessTokenSecret"]);

        }

        #endregion

        #region Events Handlers

        private static void _scryfallApiTasker_eventNewcard(object sender, ScryfallCard newItem)
        {
            if (newItem.image_url != null)
            {
                Program.WriteLine(String.Format("Sending new card {0} to everyone", newItem.name));
                try
                {
                    _telegramController.SendImageToAll(newItem);
                }
                catch (Exception ex)
                {
                    Database.InsertLog("Telegram send images to all", newItem.name, ex.ToString());
                    Program.WriteLine(String.Format("Failed to send to telegram spoil {0}", newItem.name));
                    Program.WriteLine(ex.Message);
                }

                Program.WriteLine(String.Format("Tweeting new card {0}", newItem.name));
                try
                {
                    _twitterController.PublishNewImage(newItem);
                    Database.UpdateIsSent(newItem, true);
                }
                catch (Exception ex)
                {
                    Database.InsertLog("Twitter send image", newItem.name, ex.ToString());
                    Program.WriteLine(String.Format("Failed to send to twitter spoil {0}", newItem.name));
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
