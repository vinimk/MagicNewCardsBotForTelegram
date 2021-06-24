using System;
using System.Collections.Generic;
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
        private readonly int MAX_CONCURRENT_MESSAGES = 50;
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
        async public Task InitialUpdateAsync()
        {
            await GetInitialUpdateEventsAsync();
        }

        async public Task SendImageToAllChatsAsync(Card card)
        {
            if (_idUserDebug.HasValue)
                await SendCardToChatAsync(card, new Chat { Id = _idUserDebug.Value, Type = Telegram.Bot.Types.Enums.ChatType.Private, Title = "test", FirstName = "test" });
            else
            {
                int sendingChats = 0;
                //goes trough all the chats and send a message for each one
                await foreach (Chat chat in Database.GetAllChatsAsync())
                {
                    Utils.LogInformation($"Sending {card} to {chat.Id}");
                    _ = SendCardToChatAsync(card, chat);
                    sendingChats++;
                    if (sendingChats >= MAX_CONCURRENT_MESSAGES)
                    {
                        await Task.Delay(1000);
                        sendingChats = 0;
                    }
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

        private static InputMediaPhoto CreateInputMedia(Card card)
        {


            InputMediaPhoto photo = new(new InputMedia(card.ImageUrl))
            {
                Caption = GetMessageText(card),
                ParseMode = Telegram.Bot.Types.Enums.ParseMode.Html
            };

            return photo;
        }

        private static string GetMessageText(Card card)
        {
            String messageText;
            //if the text is to big, we need to send it as a message afterwards
            Boolean isTextToBig = card.GetFullText().Length >= 1024;

            if (isTextToBig)
            {
                messageText = card.Name?.ToString();
            }
            else
            {
                messageText = card.GetTelegramTextFormatted();
            }

            return messageText;
        }

        async private Task SendCardToChatAsync(Card card, Chat chat)
        {
            try
            {
                //if there is a additional image, we must send a album
                if (card.ExtraSides != null && card.ExtraSides.Count > 0)
                {
                    List<InputMediaPhoto> lstPhotos = new();

                    InputMediaPhoto cardPhoto = CreateInputMedia(card);
                    lstPhotos.Add(cardPhoto);

                    foreach (Card extraSide in card.ExtraSides)
                    {
                        var media = CreateInputMedia(extraSide);
                        lstPhotos.Add(media);
                    }

                    await _botClient.SendMediaGroupAsync(chat, lstPhotos);
                }
                else
                {
                    var message = await _botClient.SendPhotoAsync(chatId: chat, photo: card.ImageUrl, caption: GetMessageText(card), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }
            }
            catch (Exception ex) //sometimes this exception is not a problem, like if the bot was removed from the group
            {
                if (ex.Message.Contains("bot was kicked"))
                {
                    Utils.LogInformation(String.Format("Bot was kicked from group {0}, deleting him from chat table", chat.Id));
                    await Database.DeleteFromChatAsync(chat);
                    return;
                }
                else if (ex.Message.Contains("bot was blocked by the user"))
                {
                    Utils.LogInformation(String.Format("Bot was blocked by user {0}, deleting him from chat table", chat.Id));
                    await Database.DeleteFromChatAsync(chat);
                    return;
                }
                else if (ex.Message.Contains("user is deactivated"))
                {
                    Utils.LogInformation(String.Format("User {0} deactivated, deleting him from chat table", chat.Id));
                    await Database.DeleteFromChatAsync(chat);
                    return;
                }
                else if (ex.Message.Contains("chat not found"))
                {
                    Utils.LogInformation(String.Format("Chat {0} not found, deleting him from chat table", chat.Id));
                    await Database.DeleteFromChatAsync(chat);
                    return;
                }
                else if (ex.Message.Contains("have no rights to send a message"))
                {
                    Utils.LogInformation(String.Format("Chat {0} not found, deleting him from chat table", chat.Id));
                    await Database.DeleteFromChatAsync(chat);
                    return;
                }
                else
                {
                    try
                    {
                        await Database.InsertLogAsync($"Telegram send message: {card.FullUrlWebSite} image: {card.ImageUrl}, user: {chat.FirstName} group: {chat.Title}", card.Name, ex.ToString());
                        Utils.LogInformation(ex.Message);
                        if (!String.IsNullOrEmpty(chat.FirstName))
                        {
                            Utils.LogInformation("Name: " + chat.FirstName);
                        }
                        if (!String.IsNullOrEmpty(chat.Title))
                        {
                            Utils.LogInformation("Title: " + chat.Title);
                        }

                        await _botClient.SendTextMessageAsync(23973855, $"Error on {card.FullUrlWebSite} image: {card.ImageUrl} user: {chat.FirstName} group: {chat.Title} id: {chat.Id}");
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

        async private Task GetInitialUpdateEventsAsync()
        {
            try
            {
                //get all updates
                Update[] updates = await _botClient.GetUpdatesAsync(_offset);

                if (updates.Length > 0)
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
                            if (updates.Length == 1 &&
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
                                await InsertInDbIfNotYetAddedAsync(update.Message.Chat);
                            }
                        }
                    }
                    //recursive call for offset checking
                    await GetInitialUpdateEventsAsync();
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex.Message);
                Utils.LogError(ex.StackTrace);
            }
        }

        async private Task InsertInDbIfNotYetAddedAsync(Chat chat)
        {
            //query the list to see if the chat is already in the database
            bool isInDb = await Database.ChatExistsAsync(chat);
            if (isInDb == false)
            {
                //if it isn't adds it
                await Database.InsertChatAsync(chat);
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
                    InsertInDbIfNotYetAddedAsync(args.Update.Message.Chat).Wait();
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