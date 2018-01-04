using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using System.IO;
using Telegram.Bot.Exceptions;
using System.Linq;
using Telegram.Bot.Args;

namespace MagicBot
{

    public class TelegramController
    {
        #region Definitions
        private String _telegramBotApiKey;
        private Telegram.Bot.TelegramBotClient _botClient;
        private Int32 _offset;
        #endregion

        #region Constructors
        public TelegramController(String apiKey)
        {
            _telegramBotApiKey = apiKey;
            _botClient = new Telegram.Bot.TelegramBotClient(_telegramBotApiKey);
            _offset = 0;
        }
        #endregion

        #region Public Methods
        public void InitialUpdate()
        {
            GetInitialUpdateEvents();
        }

        public void SendImageToAll(Card card)
        {
            //goes trough all the chats and send a message for each one
            List<Chat> lstChat = Database.GetAllChats();
            // List<Chat> lstChat = new List<Chat>(){
            //     new Chat()
            //     {
            //         Id = -1001108555769,
            //     }
            // };
            foreach (Chat chat in lstChat)
            {
                SendSpoilToChat(card, chat);
            }
        }


        public void HookUpdateEvent()
        {
            //removes then adds the handler, that way it make sure that the event is handled
            _botClient.OnUpdate -= botClientOnUpdate;
            _botClient.OnUpdate += botClientOnUpdate;
            _botClient.StartReceiving();
        }


        #endregion

        #region Private Methods

        private void SendSpoilToChat(Card card, Chat chat)
        {
            try
            {
                int replyToMessage = 0;
                {
                    String messageText;
                    //if the text is to big, we need to send it as a message afterwards
                    Boolean isTextToBig = card.GetTelegramText().Length >= 200;

                    if (isTextToBig)
                    {
                        messageText = card.Name;
                    }
                    else
                    {
                        messageText = card.GetTelegramText();
                    }

                    //try to send directly, if it fails we download then upload it
                    try
                    {
                        Task<Message> task = _botClient.SendPhotoAsync(chat, new FileToSend(new Uri(card.ImageUrl)), messageText, false, replyToMessage);
                        task.Wait();
                        replyToMessage = task.Result.MessageId;
                    }
                    catch
                    {
                        Stream stream = Program.GetImageFromUrl(card.ImageUrl);
                        Task<Message> task = _botClient.SendPhotoAsync(chat, new FileToSend(card.Name + ".PNG", stream), messageText, false, replyToMessage);
                        task.Wait();
                        replyToMessage = task.Result.MessageId;
                    }

                    if (isTextToBig)
                    {
                        Task<Message> task = _botClient.SendTextMessageAsync(chat, card.GetTelegramTextFormatted(), Telegram.Bot.Types.Enums.ParseMode.Html, false, false, replyToMessage);
                        task.Wait();
                        replyToMessage = task.Result.MessageId;
                    }

                }


                //if there is a additional image, we send it as a reply
                if (card.ExtraSides != null && card.ExtraSides.Count > 0)
                {
                    foreach (Card extraSide in card.ExtraSides)
                    {
                        String messageText;
                        Boolean isTextToBig = extraSide.GetTelegramText().Length >= 200;

                        if (isTextToBig)
                        {
                            messageText = extraSide.Name;
                        }
                        else
                        {
                            messageText = extraSide.GetTelegramText();
                        }

                        //try to send directly, if it fails we download then upload it
                        try
                        {
                            Task<Message> task = _botClient.SendPhotoAsync(chat, new FileToSend(new Uri(extraSide.ImageUrl)), messageText, false, replyToMessage);
                            task.Wait();
                            replyToMessage = task.Result.MessageId;
                        }
                        catch
                        {
                            Stream stream = Program.GetImageFromUrl(extraSide.ImageUrl);
                            Task<Message> task = _botClient.SendPhotoAsync(chat, new FileToSend(extraSide.Name + ".PNG", stream), messageText, false, replyToMessage);
                            task.Wait();
                            replyToMessage = task.Result.MessageId;
                        }

                        if (isTextToBig)
                        {
                            Task<Message> task = _botClient.SendTextMessageAsync(chat, extraSide.GetTelegramTextFormatted(), Telegram.Bot.Types.Enums.ParseMode.Html, false, false, replyToMessage);
                            task.Wait();
                            replyToMessage = task.Result.MessageId;
                        }
                    }

                }

            }
            catch (Exception ex) //sometimes this exception is not a problem, like if the bot was removed from the group
            {
                if (ex.Message.Contains("bot was kicked"))
                {
                    Program.WriteLine(String.Format("Bot was kicked from group {0}, consider deletting him from the database on table Chats", chat.Title));
                    return;
                }
                else if (ex.Message.Contains("bot was blocked by the user"))
                {
                    Program.WriteLine(String.Format("Bot was blocked by user {0}, consider deletting him from the database on table Chats", chat.FirstName));
                    return;
                }
                else
                {
                    Database.InsertLog("Telegram send message", card.Name, ex.ToString());
                    Program.WriteLine(ex.Message);
                    Program.WriteLine(ex.StackTrace);
                }
            }
        }



        private void GetInitialUpdateEvents()
        {
            //get all updates
            Task<Update[]> taskTelegramUpdates = _botClient.GetUpdatesAsync(_offset);
            taskTelegramUpdates.Wait();
            Update[] updates = taskTelegramUpdates.Result;

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
                            AddIfNeeded(update.Message.Chat);
                        }
                    }
                }
                //recursive call for offset checking
                GetInitialUpdateEvents();
            }
        }

        private void AddIfNeeded(Chat chat)
        {
            //query the list to see if the chat is already in the database
            if (!Database.IsChatInDatabase(chat))
            {
                //if it isn't adds it
                Database.InsertChat(chat);
                _botClient.SendTextMessageAsync(chat, "Bot initialized sucessfully, new cards will be sent when avaliable").Wait();
                Program.WriteLine(String.Format("Chat {0} - {1}{2} added", chat.Id, chat.Title, chat.FirstName));
            }
        }
        #endregion

        #region Events
        private void botClientOnUpdate(object sender, UpdateEventArgs args)
        {
            if (args != null &&
                args.Update != null &&
                args.Update.Message != null &&
                args.Update.Message.Chat != null)
            {
                Program.WriteLine(String.Format("Handling event ID:{0} from user {1}{2}", args.Update.Id, args.Update.Message.Chat.FirstName, args.Update.Message.Chat.Title));
                AddIfNeeded(args.Update.Message.Chat);
                _offset = args.Update.Id;
            }
        }
        #endregion
    }
}