using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using ImageSharp;
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

        public void SendImageToAll(SpoilItem spoil)
        {
            SendImage(spoil);
        }
        #endregion

        #region Private Methods
        private List<Chat> GetChatList()
        {
            Task<List<Chat>> taskDb = Database.GetAllChats();
            taskDb.Wait();
            return taskDb.Result;
        }

        public void HookUpdateEvent()
        {
            //removes then adds the handler, that way it make sure that the event is handled
            _botClient.OnUpdate -= botClientOnUpdate;
            _botClient.OnUpdate += botClientOnUpdate;
            _botClient.StartReceiving();
        }

        private void SendImage(SpoilItem spoil)
        {
            //goes trough all the chats and send a message for each one
            foreach (Chat chat in GetChatList())
            {
                try
                {
                    Message firstMessage;

                    //gets a temp file for the image
                    String pathTempImage = System.IO.Path.GetTempFileName();
                    //saves the image in the disk in the temp file
                    FileStream fileStream = new FileStream(pathTempImage, FileMode.OpenOrCreate);
                    spoil.Image.Save(fileStream, ImageSharp.ImageFormats.Png);
                    fileStream.Flush();
                    fileStream.Close();

                    //loads the image and sends it
                    using (var stream = System.IO.File.Open(pathTempImage, FileMode.Open))
                    {
                        FileToSend fts = new FileToSend();
                        fts.Content = stream;
                        fts.Filename = pathTempImage.Split('\\').Last();
                        Task<Message> task = _botClient.SendPhotoAsync(chat, fts, spoil.TelegramText());
                        task.Wait();
                        //stores this if we need to send a second image
                        firstMessage = task.Result;
                    }
                    //if there is a additional image, we send it as a reply
                    if (spoil.AdditionalImage != null)
                    {
                        String pathTempImageAdditional = System.IO.Path.GetTempFileName();
                        //saves the image in the disk in the temp file
                        FileStream fileStreamAdditional = new FileStream(pathTempImageAdditional, FileMode.OpenOrCreate);
                        spoil.Image.Save(fileStreamAdditional, ImageSharp.ImageFormats.Png);
                        fileStream.Flush();
                        fileStream.Close();

                        //loads the image and sends it
                        using (var stream = System.IO.File.Open(pathTempImageAdditional, FileMode.Open))
                        {
                            FileToSend fts = new FileToSend();
                            fts.Content = stream;
                            fts.Filename = pathTempImageAdditional.Split('\\').Last();
                            _botClient.SendPhotoAsync(chat, fts, spoil.TelegramText(), false, firstMessage.MessageId).Wait();
                        }
                    }
                }
                catch (Exception ex) //sometimes this exception is not a problem, like if the bot was removed from the group
                {
                    if (ex.Message.Contains("bot was kicked"))
                    {
                        Console.WriteLine(String.Format("Bot was kicked from group {0}, consider setting isDeleted to true on table Chats", chat.Title));
                        continue;
                    }
                    if (ex.Message.Contains("banned"))
                    {
                        Console.WriteLine(String.Format("Bot was banned by user {0}, consider setting isDeleted to true on table Chats", chat.FirstName));
                        continue;
                    }
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
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
            Task<Boolean> taskDb = Database.IsChatInDatabase(chat);
            taskDb.Wait();

            //if it isn't adds it 
            if (!taskDb.Result)
            {
                Database.InsertChat(chat).Wait();
                _botClient.SendTextMessageAsync(chat, "Bot initialized sucessfully, new cards will be sent when avaliable").Wait();
                Console.WriteLine(String.Format("Chat {0} - {1}{2} added", chat.Id, chat.Title, chat.FirstName));
                //after adding a item in the database, update the list 
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
                Console.WriteLine(String.Format("Handling event ID:{0} from user {1}{2}", args.Update.Id, args.Update.Message.Chat.FirstName, args.Update.Message.Chat.Title));
                AddIfNeeded(args.Update.Message.Chat);
                _offset = args.Update.Id;
            }
        }
        #endregion
    }
}