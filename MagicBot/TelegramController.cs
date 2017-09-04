using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using ImageSharp;
using System.IO;
using Telegram.Bot.Exceptions;
using System.Linq;

namespace MagicBot
{

    public class TelegramController
    {
        #region Definitions
        private String _telegramBotApiKey;
        private Telegram.Bot.TelegramBotClient _botClient;
        private Database _db;
        private Int32 _offset;
        #endregion

        #region Constructors
        public TelegramController(String apiKey, Database db)
        {
            _telegramBotApiKey = apiKey;
            _botClient = new Telegram.Bot.TelegramBotClient(_telegramBotApiKey);
            _db = db;
            _offset = 0;
        }
        #endregion

        #region Public Methods
        public void Update()
        {
            UpdateChatList().Wait();
        }

        public void SendImageToAll(Image<Rgba32> image, String caption = "")
        {
            SendImage(image, caption).Wait();
        }

        #endregion

        #region Private Methods
        private async Task SendImage(Image<Rgba32> image, String caption = "")
        {
            //get all chats from the database
            Task<List<Chat>> taskDb = _db.GetAllChats();
            taskDb.Wait();
            List<Chat> lstChats = taskDb.Result;

            //goes trough all the chats and send a message for each one
            foreach (Chat chat in lstChats)
            {
                try
                {
                    //gets a temp file for the image
                    String pathTempImage = System.IO.Path.GetTempFileName();
                    //saves the image in the disk in the temp file
                    FileStream fileStream = new FileStream(pathTempImage, FileMode.OpenOrCreate);
                    image.Save(fileStream, ImageSharp.ImageFormats.Png);
                    fileStream.Flush();
                    fileStream.Close();

                    //loads the image and sends it
                    using (var stream = System.IO.File.Open(pathTempImage, FileMode.Open))
                    {
                        FileToSend fts = new FileToSend();
                        fts.Content = stream;
                        fts.Filename = pathTempImage.Split('\\').Last();
                        await _botClient.SendPhotoAsync(chat, fts, caption);
                    }
                }
                catch (ApiRequestException ex) //sometimes this exception is not a problem, like if the bot was removed from the group
                {
                    if(ex.Message.Contains("bot was kicked"))
                    {
                        Console.WriteLine(String.Format("Bot was kicked from group {0}, consider setting isDeleted to true on table Chats", chat.Title));
                        continue;
                    }
                    Console.WriteLine(ex.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception("sendImage", ex);
                }
            }


        }

        private async Task UpdateChatList()
        {
            //get all updates
            Update[] updates = await _botClient.GetUpdatesAsync(_offset);

            if (updates.Count() > 0)
            {
                Task<List<Chat>> taskDb = _db.GetAllChats();
                taskDb.Wait();
                List<Chat> lstChats = taskDb.Result;

                //check if the chatID is in the list, if it isn't, adds it
                foreach (Update update in updates)
                {
                    if (update != null)
                    {
                        //if the offset is equal to the update
                        //and there is only one message in the return
                        //it means that there are no new messages after the offset
                        //so we can return
                        if( updates.Count() == 1 &&
                            _offset == update.Id)
                        {
                            return;
                        }
                        //else we have to continue updating
                        else
                        {
                            _offset = update.Id;
                        }

                        if (update.Message != null &&
                            update.Message.Chat != null)
                        {
                            //query the list to see if the chat is already in the database
                            //if it isn't adds it 
                            var data = lstChats.Where(x => x.Id.Equals(update.Message.Chat.Id));

                            if (data.Count() == 0)
                            {
                                await _db.InsertChat(update.Message.Chat);
                                await _botClient.SendTextMessageAsync(update.Message.Chat, "Bot initialized sucessfully, new cards will be sent when avaliable");
                            }

                        }
                    }
                }
                //recursive call for offset checking
                await UpdateChatList();
            }
        }
            #endregion

        }
    }