using MagicNewCardsBot.Helpers;
using MagicNewCardsBot.StorageClasses;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace MagicNewCardsBot.TaskersAndControllers
{

    public class TelegramController
    {
        #region Definitions
        private readonly string _telegramBotApiKey;
        private readonly TelegramBotClient _botClient;
        private readonly int MAX_CONCURRENT_MESSAGES = 50;
        private int _offset;
        private readonly long? _idUserDebug;
        #endregion

        #region Constructors
        public TelegramController(string apiKey, long? IdUserDebug = null)
        {
            _telegramBotApiKey = apiKey;
            _botClient = new TelegramBotClient(_telegramBotApiKey);
            _offset = 0;
            _idUserDebug = IdUserDebug;
        }

        #endregion

        #region Public Methods
        public async Task InitialUpdateAsync()
        {
            await GetInitialUpdateEventsAsync();
        }

        private async Task SendCardToChatsAsync(Card card, IAsyncEnumerable<Chat> chats)
        {
            int sendingChats = 0;
            //go through all the chats and send a message for each one
            await foreach (Chat chat in chats)
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

        public async Task SendImageToAllChatsAsync(Card card)
        {
            if (_idUserDebug.HasValue)
            {
                await SendCardToChatAsync(card, new Chat { Id = _idUserDebug.Value, Type = Telegram.Bot.Types.Enums.ChatType.Private, Title = "test", FirstName = "test" });
            }
            else
            {
                await SendCardToChatsAsync(card, Database.GetChatsAsync());
            }
        }

        public async Task SendImageToChatsByRarityAsync(Card card)
        {
            await SendCardToChatsAsync(card, Database.GetChatsAsync(card.GetRarityCharacter()));
        }

        public void HookUpdateEvent()
        {
            CancellationTokenSource cts = new();
            CancellationToken cancellationToken = cts.Token;
            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, null, cancellationToken);
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
            string messageText;
            //if the text is to big, we need to send it as a message afterwards
            bool isTextToBig = card.GetFullText().Length >= 1024;

            messageText = isTextToBig ? (card.Name?.ToString()) : card.GetTelegramTextFormatted();

            return messageText;
        }

        private async Task SendCardToChatAsync(Card card, Chat chat)
        {
            try
            {
                //if there is an additional image, we must send an album
                if (card.ExtraSides != null && card.ExtraSides.Count > 0)
                {
                    List<InputMediaPhoto> lstPhotos = new();

                    InputMediaPhoto cardPhoto = CreateInputMedia(card);
                    lstPhotos.Add(cardPhoto);

                    foreach (Card extraSide in card.ExtraSides)
                    {
                        InputMediaPhoto media = CreateInputMedia(extraSide);
                        lstPhotos.Add(media);
                    }

                    _ = await _botClient.SendMediaGroupAsync(chat, lstPhotos);
                }
                else
                {
                    Message message = await _botClient.SendPhotoAsync(chatId: chat, photo: card.ImageUrl, caption: GetMessageText(card), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }
            }
            catch (Exception ex) //sometimes this exception is not a problem, like if the bot was removed from the group
            {
                if (ex.Message.Contains("bot was kicked"))
                {
                    Utils.LogInformation(string.Format("Bot was kicked from group {0}, deleting him from chat table", chat.Id));
                    _ = await Database.DeleteFromChatAsync(chat);
                    return;
                }
                else if (ex.Message.Contains("bot was blocked by the user"))
                {
                    Utils.LogInformation(string.Format("Bot was blocked by user {0}, deleting him from chat table", chat.Id));
                    _ = await Database.DeleteFromChatAsync(chat);
                    return;
                }
                else if (ex.Message.Contains("user is deactivated"))
                {
                    Utils.LogInformation(string.Format("User {0} deactivated, deleting him from chat table", chat.Id));
                    _ = await Database.DeleteFromChatAsync(chat);
                    return;
                }
                else if (ex.Message.Contains("chat not found"))
                {
                    Utils.LogInformation(string.Format("Chat {0} not found, deleting him from chat table", chat.Id));
                    _ = await Database.DeleteFromChatAsync(chat);
                    return;
                }
                else if (ex.Message.Contains("have no rights to send a message"))
                {
                    Utils.LogInformation(string.Format("Chat {0} not found, deleting him from chat table", chat.Id));
                    _ = await Database.DeleteFromChatAsync(chat);
                    return;
                }
                else
                {
                    try
                    {
                        _ = await Database.InsertLogAsync($"Telegram send message: {card.FullUrlWebSite} image: {card.ImageUrl}, user: {chat.FirstName} group: {chat.Title}", card.Name, ex.ToString());
                        Utils.LogInformation(ex.Message);
                        if (!string.IsNullOrEmpty(chat.FirstName))
                        {
                            Utils.LogInformation("Name: " + chat.FirstName);
                        }
                        if (!string.IsNullOrEmpty(chat.Title))
                        {
                            Utils.LogInformation("Title: " + chat.Title);
                        }

                        _ = await _botClient.SendTextMessageAsync(23973855, $"Error on {card.FullUrlWebSite} image: {card.ImageUrl} user: {chat.FirstName} group: {chat.Title} id: {chat.Id}");
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

        private async Task GetInitialUpdateEventsAsync()
        {
            try
            {
                //get all updates
                Update[] updates = await _botClient.GetUpdatesAsync(_offset);

                if (updates.Length > 0)
                {
                    //check if the chatID is in the list, if it isn't, add it
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

        private async Task InsertInDbIfNotYetAddedAsync(Chat chat)
        {
            //query the list to see if the chat is already in the database
            bool isInDb = await Database.ChatExistsAsync(chat);
            if (isInDb == false)
            {
                //if it's not, add it
                _ = await Database.InsertChatAsync(chat);
                _ = await _botClient.SendTextMessageAsync(chat, "Bot has been initialized successfully, new cards will be sent when available");
                Utils.LogInformation(string.Format("Chat {0} - {1}{2} added", chat.Id, chat.Title, chat.FirstName));
            }
        }
        #endregion

        #region Events

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is Message message)
            {
                try
                {
                    Utils.LogInformation(string.Format("Handling event ID:{0} from user {1}{2}", message.MessageId, message.Chat.FirstName, message.Chat.Title));
                    await InsertInDbIfNotYetAddedAsync(message.Chat);

                    //commands handling
                    if (message.EntityValues != null)
                    {
                        foreach (string entity in message.EntityValues)
                        {
                            if (entity.Contains($"/rarity") && message.Text != null)
                            {
                                string value = message.Text.Replace(entity, string.Empty);
                                string validString = Utils.ReturnValidRarityFromCommand(value);
                                if (!string.IsNullOrWhiteSpace(validString))
                                {
                                    await Database.UpdateWantedRaritiesForChatAsync(message.Chat, validString);
                                    _ = await _botClient.SendTextMessageAsync(message.Chat, "Updated rarities that will be received to: " + validString, cancellationToken: cancellationToken);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex.Message);
                    Utils.LogError(ex.StackTrace);
                }
            }
        }

        private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                Utils.LogError(apiRequestException.Message);
                Utils.LogError(apiRequestException.StackTrace);
                _ = await Database.InsertLogAsync("HandleErrorAsync", string.Empty, exception.ToString());
            }
        }

        #endregion
    }
}
