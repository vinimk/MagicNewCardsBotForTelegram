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
        private String _telegramBotApiKey;
        private List<Int64> _chatIds;
        private Telegram.Bot.TelegramBotClient _botClient;

        public TelegramController(String apiKey)
        {
            _telegramBotApiKey = apiKey;
            _botClient = new Telegram.Bot.TelegramBotClient(_telegramBotApiKey);
            _chatIds = new List<Int64>();
        }

        public void Update()
        {
            updateChatList().Wait();
        }

        public void SendImageToAll(Image<Rgba32> image, String caption = "")
        {
            sendImage(image, caption).Wait();
        }

        private async Task sendImage(Image<Rgba32> image, String caption = "")
        {
            //goes trough all the chats and send a message for each one
            foreach (Int64 id in _chatIds)
            {
                try
                {
                    String pathTempImage = System.IO.Path.GetTempFileName();
                    FileStream fileStream = new FileStream(pathTempImage, FileMode.OpenOrCreate);

                    image.Save(fileStream, ImageSharp.ImageFormats.Png);
                    fileStream.Flush();
                    fileStream.Close();

                    using (var stream = System.IO.File.Open(pathTempImage, FileMode.Open))
                    {
                        FileToSend fts = new FileToSend();
                        fts.Content = stream;
                        fts.Filename = pathTempImage.Split('\\').Last();
                        var test = await _botClient.SendPhotoAsync(id, fts, caption);
                    }

                }
                catch (ApiRequestException ex) //sometimes this exception is not a problem, like if the bot was removed from the group
                {
                    Console.WriteLine(ex.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception("sendImage", ex);
                }
            }


        }
        private async Task updateChatList()
        {
            //get all updates
            Update[] updates = await _botClient.GetUpdatesAsync(0, Int32.MaxValue);

            //check if the chatID is in the list, if it isn't, adds it
            foreach (Update u in updates)
            {
                if (u != null &&
                    u.Message != null &&
                    u.Message.Chat != null)
                {
                    if (!_chatIds.Contains(u.Message.Chat.Id))
                    {
                        _chatIds.Add(u.Message.Chat.Id);
                    }
                }
            }

        }

    }
}