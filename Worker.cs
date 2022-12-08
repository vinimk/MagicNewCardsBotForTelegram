using MagicNewCardsBot.Helpers;
using MagicNewCardsBot.StorageClasses;
using MagicNewCardsBot.TaskersAndControllers;

namespace MagicNewCardsBot
{
    public class Worker : BackgroundService
    {

        private readonly ILogger<Worker> logger;

        public Worker(ILogger<Worker> logger, WorkerOptions options)
        {
            this.logger = logger;
            Utils.SetLogger(this.logger);

            Utils.LogInformation("Initializing");
            Utils.LogInformation($"DebugMode: {options.IsDebugMode}");

            _isDebugMode = options.IsDebugMode;
            _tasker = new MTGPicsTasker();

            TimeInternalMS = options.TimeExecuteIntervalInMs;

            Database.SetConnectionString(options.ConnectionStringMySQL);

            TelegramController = _isDebugMode
                ? new TelegramController(options.TelegramBotApiKey, options.TelegramDebugUserID)
                : new TelegramController(options.TelegramBotApiKey);



            TwitterController = new TwitterController(options.TwitterConsumerKey, options.TwitterConsumerSecret, options.TwitterAcessToken, options.TwitterAcessTokenSecret);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_isDebugMode)
            {
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
                    Utils.LogInformation(string.Format("Going to sleep for {0} ms.", TimeInternalMS));
                    await Task.Delay(TimeInternalMS, stoppingToken);
                }
                catch (Exception ex)
                {
                    try
                    {
                        _ = await Database.InsertLogAsync("Getting new cards", string.Empty, ex.ToString());
                        Utils.LogInformation(ex.ToString());
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine("Exception in catch, sad");
                        Console.WriteLine(ex2.ToString());
                    }
                    await Task.Delay(TimeInternalMS, stoppingToken);
                }
            }
        }

        private static async Task DistributeCardAsync(Card card)
        {
            if (card.ImageUrl != null)
            {
                if (!_isDebugMode)
                {
                    await Database.UpdateIsSentAsync(card, true);
                }

                if (card.SendTo is SendTo.Both or
                    SendTo.OnlyAll)
                {
                    try
                    {
                        if (!_isDebugMode)
                        {
                            Utils.LogInformation(string.Format("Tweeting new card {0}", card.Name));
                            await TwitterController.TweetCardAsync(card);
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = await Database.InsertLogAsync("Twitter send image", card.Name, ex.ToString());
                        Utils.LogInformation(string.Format("Failed to send to twitter spoil {0}", card.Name));
                        Utils.LogInformation(ex.Message);
                        Utils.LogInformation(ex.StackTrace);
                    }

                    try
                    {
                        await TelegramController.SendImageToAllChatsAsync(card);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            _ = await Database.InsertLogAsync("Telegram send images to all", card.Name, ex.ToString());
                        }
                        catch { } //if there is any error here, we don't wanna stop the bot
                        Utils.LogInformation(string.Format("Failed to send to telegram spoil {0}", card.Name));
                        Utils.LogInformation(ex.Message);
                        Utils.LogInformation(ex.StackTrace);
                    }
                }

                if (card.SendTo is SendTo.Both or
                   SendTo.OnlyRarity)
                {
                    try
                    {
                        await TelegramController.SendImageToChatsByRarityAsync(card);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            _ = await Database.InsertLogAsync("Telegram send images to rarity", card.Name, ex.ToString());
                        }
                        catch { } //if there is any error here, we don't wanna stop the bot
                        Utils.LogInformation(string.Format("Failed to send to telegram rarirty spoil {0}", card.Name));
                        Utils.LogInformation(ex.Message);
                        Utils.LogInformation(ex.StackTrace);
                    }
                }
            }
        }


        #region Definitions
        private static Tasker _tasker;
        private static bool _isDebugMode;

        public static int TimeInternalMS { get; set; }
        public static TwitterController TwitterController { get; set; }
        public static TelegramController TelegramController { get; set; }
        #endregion


    }
}
