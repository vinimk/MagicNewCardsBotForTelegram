using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace MagicNewCardsBot
{

    public class TelegramController
    {
        #region Definitions
        private readonly string _telegramBotApiKey;
        private readonly Telegram.Bot.TelegramBotClient _botClient;
        private int _offset;
        private readonly long? _idUserDebug;
        #endregion

        #region Constructors
        public TelegramController(String apiKey, long? IdUserDebug = null)
        {
            _telegramBotApiKey = apiKey;
            _botClient = new Telegram.Bot.TelegramBotClient(_telegramBotApiKey);
            _offset = 0;
            _idUserDebug = IdUserDebug;
        }

        #endregion

        #region Public Methods
        async public Task InitialUpdate()
        {
            await GetInitialUpdateEvents();
        }

        async public Task SendImageToAll(Card card)
        {
            if (_idUserDebug.HasValue)
                _ = SendSpoilToChat(card, new Chat { Id = _idUserDebug.Value, Type = Telegram.Bot.Types.Enums.ChatType.Private, Title = "test", FirstName = "test" });
            else
            {
                //goes trough all the chats and send a message for each one
                await foreach (Chat chat in Database.GetAllChats())
                {
                    Utils.LogInformation($"Sending{card} to {chat.Id}");
                    _ = SendSpoilToChat(card, chat);
                }
            }
        }

        public void HookUpdateEvent()
        {
            //removes then adds the handler, that way it make sure that the event is handled
            _botClient.OnUpdate -= BotClientOnUpdate;
            _botClient.OnUpdate += BotClientOnUpdate;
            _botClient.StartReceiving();
        }


        #endregion

        #region Private Methods

        async private Task SendSpoilToChat(Card card, Chat chat)
        {
            try
            {
                int replyToMessage;
                String messageText;
                //if the text is to big, we need to send it as a message afterwards
                Boolean isTextToBig = card.GetFullText().Length > 1024;

                if (isTextToBig)
                {
                    messageText = card.Name?.ToString();
                }
                else
                {
                    messageText = card.GetTelegramTextFormatted();
                }

                {
                    var message = await _botClient.SendPhotoAsync(chatId: chat, photo: card.ImageUrl, caption: messageText, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    replyToMessage = message.MessageId;
                    if (isTextToBig)
                    {
                        await Task.Delay(500);
                        message = await _botClient.SendTextMessageAsync(chat, card.GetTelegramTextFormatted(), Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: replyToMessage);
                        replyToMessage = message.MessageId;
                    }
                }


                //if there is a additional image, we send it as a reply
                if (card.ExtraSides != null && card.ExtraSides.Count > 0)
                {
                    foreach (Card extraSide in card.ExtraSides)
                    {
                        isTextToBig = extraSide.GetFullText().Length >= 1024;

                        if (isTextToBig)
                        {
                            messageText = extraSide.Name;
                        }
                        else
                        {
                            messageText = extraSide.GetTelegramTextFormatted();
                        }

                        var message = await _botClient.SendPhotoAsync(chatId: chat, photo: extraSide.ImageUrl, caption: messageText, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: replyToMessage);
                        replyToMessage = message.MessageId;

                        if (isTextToBig)
                        {
                            await Task.Delay(500);
                            message = await _botClient.SendTextMessageAsync(chat, extraSide.GetTelegramTextFormatted(), Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: replyToMessage);
                            replyToMessage = message.MessageId;
                        }

                    }
                }
            }
            catch (Exception ex) //sometimes this exception is not a problem, like if the bot was removed from the group
            {
                if (ex.Message.Contains("bot was kicked"))
                {
                    Utils.LogInformation(String.Format("Bot was kicked from group {0}, deleting him from chat table", chat.Id));
                    await Database.DeleteFromChat(chat);
                    return;
                }
                else if (ex.Message.Contains("bot was blocked by the user"))
                {
                    Utils.LogInformation(String.Format("Bot was blocked by user {0}, deleting him from chat table", chat.Id));
                    await Database.DeleteFromChat(chat);
                    return;
                }
                else if (ex.Message.Contains("user is deactivated"))
                {
                    Utils.LogInformation(String.Format("User {0} deactivated, deleting him from chat table", chat.Id));
                    await Database.DeleteFromChat(chat);
                    return;
                }
                else if (ex.Message.Contains("chat not found"))
                {
                    Utils.LogInformation(String.Format("Chat {0} not found, deleting him from chat table", chat.Id));
                    await Database.DeleteFromChat(chat);
                    return;
                }
                else
                {
                    try
                    {
                        await Database.InsertLog($"Telegram send message: {card.FullUrlWebSite} image: {card.ImageUrl}, user: {chat.FirstName} group: {chat.Title}", card.Name, ex.ToString());
                        Utils.LogInformation(ex.Message);
                        if (!String.IsNullOrEmpty(chat.FirstName))
                        {
                            Utils.LogInformation("Name: " + chat.FirstName);
                        }
                        if (!String.IsNullOrEmpty(chat.Title))
                        {
                            Utils.LogInformation("Title: " + chat.Title);
                        }

                        await _botClient.SendTextMessageAsync(23973855, $"Error on {card.FullUrlWebSite} image: {card.ImageUrl} user: {chat.FirstName} group: {chat.Title}");
                        Utils.LogError(ex.StackTrace);
                        return;
                    }
                    catch
                    {
                        Utils.LogError("Error on SendSpoilToChat catch clause");
                    } //if there is any error here, we do not want to stop sending the other cards, so just an empty catch
                }
            }
        }

        async private Task GetInitialUpdateEvents()
        {
            try
            {
                //get all updates
                Update[] updates = await _botClient.GetUpdatesAsync(_offset);

                if (updates.Count() > 0)
                {
                    //check if the chatID is in the list, if it isn't, adds it
                    foreach (Update update in updates)
                    {
                        if (update != null)
                        {
                            //if the offset is equal to the update
                            //and there is only one message in the return
                            //it means that there are no new messages after the offset
                            //so we can stop this and add the hook for the on update event
                            if (updates.Count() == 1 &&
                                _offset == update.Id)
                            {
                                HookUpdateEvent();
                                return;
                            }
                            //else we have to continue updating
                            else
                            {
                                _offset = update.Id;
                            }

                            //check if the message is in good state
                            if (update.Message != null &&
                                update.Message.Chat != null)
                            {
                                //call the method to see if it is needed to add it to the database
                                await AddIfNeeded(update.Message.Chat);
                            }
                        }
                    }
                    //recursive call for offset checking
                    await GetInitialUpdateEvents();
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex.Message);
                Utils.LogError(ex.StackTrace);
            }
        }

        async private Task AddIfNeeded(Chat chat)
        {
            //query the list to see if the chat is already in the database
            bool isInDb = await Database.IsChatInDatabase(chat);
            if (isInDb == false)
            {
                //if it isn't adds it
                await Database.InsertChat(chat);
                await _botClient.SendTextMessageAsync(chat, "Bot initialized sucessfully, new cards will be sent when avaliable");
                Utils.LogInformation(String.Format("Chat {0} - {1}{2} added", chat.Id, chat.Title, chat.FirstName));
            }
        }
        #endregion

        #region Events
        private void BotClientOnUpdate(object sender, UpdateEventArgs args)
        {
            try
            {
                if (args != null &&
                    args.Update != null &&
                    args.Update.Message != null &&
                    args.Update.Message.Chat != null)
                {
                    Utils.LogInformation(String.Format("Handling event ID:{0} from user {1}{2}", args.Update.Id, args.Update.Message.Chat.FirstName, args.Update.Message.Chat.Title));
                    AddIfNeeded(args.Update.Message.Chat).Wait();
                    _offset = args.Update.Id;
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex.Message);
                Utils.LogError(ex.StackTrace);
            }

        }
        #endregion
    }
}