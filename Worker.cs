using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MagicNewCardsBot
{
    public class Worker : BackgroundService
    {

        private readonly ILogger<Worker> logger;
        private readonly WorkerOptions options;

        public Worker(ILogger<Worker> logger, WorkerOptions options)
        {
            this.logger = logger;
            this.options = options;
            Utils.SetLogger(this.logger);

            Utils.LogInformation("Initializing");
            Utils.LogInformation($"DebugMode: {options.IsDebugMode}");

            _isDebugMode = options.IsDebugMode;
            _tasker = new MTGPicsTasker();

            TimeInternalMS = options.TimeExecuteIntervalInMs;

            Database.SetConnectionString(options.ConnectionStringMySQL);

            if (_isDebugMode)
                TelegramController = new TelegramController(options.TelegramBotApiKey, options.TelegramDebugUserID);
            else
                TelegramController = new TelegramController(options.TelegramBotApiKey);



            TwitterController = new TwitterController(options.TwitterConsumerKey, options.TwitterConsumerSecret, options.TwitterAcessToken, options.TwitterAcessTokenSecret);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_isDebugMode)
            {
                await TelegramController.InitialUpdateAsync();
                TelegramController.HookUpdateEvent();
            }
            else
            {
                Utils.LogInformation("********************* DEBUG MODE ****************");
            }


            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //we get the new cards
                    //note that since we have a event handler for new cards, the event will be fired if a new card is found
                    Utils.LogInformation("Getting new cards");
                    await foreach (Card newCard in _tasker.GetNewCardsAsync())
                    {
                        await DistributeCardAsync(newCard);
                    }

                    //we wait for a while before executing again, this interval be changed in the appsettings.json file
                    Utils.LogInformation(String.Format("Going to sleep for {0} ms.", TimeInternalMS));
                    await Task.Delay(TimeInternalMS);
                }
                catch (Exception ex)
                {
                    try
                    {
                        await Database.InsertLogAsync("Getting new cards", String.Empty, ex.ToString());
                        Utils.LogInformation(ex.ToString());
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

        async private static Task DistributeCardAsync(Card newItem)
        {
            if (newItem.ImageUrl != null)
            {
                if (!_isDebugMode)
                    await Database.UpdateIsSentAsync(newItem, true);


                try
                {
                    if (!_isDebugMode)
                    {
                        Utils.LogInformation(String.Format("Tweeting new card {0}", newItem.Name));
                        await TwitterController.TweetCardAsync(newItem);
                    }
                }
                catch (Exception ex)
                {
                    await Database.InsertLogAsync("Twitter send image", newItem.Name, ex.ToString());
                    Utils.LogInformation(String.Format("Failed to send to twitter spoil {0}", newItem.Name));
                    Utils.LogInformation(ex.Message);
                    Utils.LogInformation(ex.StackTrace);
                }

                try
                {
                    await TelegramController.SendImageToAllChatsAsync(newItem);
                }
                catch (Exception ex)
                {
                    try
                    {
                        await Database.InsertLogAsync("Telegram send images to all", newItem.Name, ex.ToString());
                    }
                    catch { } //if there is any error here, we don't wanna stop the bot
                    Utils.LogInformation(String.Format("Failed to send to telegram spoil {0}", newItem.Name));
                    Utils.LogInformation(ex.Message);
                    Utils.LogInformation(ex.StackTrace);
                }
            }
        }


        #region Definitions
        private static Tasker _tasker;
        private static TelegramController _telegramController;
        private static TwitterController _twitterController;
        private static int _timeInternalMS;
        private static bool _isDebugMode;

        public static int TimeInternalMS { get => _timeInternalMS; set => _timeInternalMS = value; }
        public static TwitterController TwitterController { get => _twitterController; set => _twitterController = value; }
        public static TelegramController TelegramController { get => _telegramController; set => _telegramController = value; }
        #endregion


    }
}
