using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MagicNewCardsBot.Helpers
{
    public static class Utils
    {
        private static ILogger<Worker> logger;
        async public static Task<Stream> GetStreamFromUrlAsync(string url)
        {
            //do a webrequest to get the image
            using HttpClient client = new();
            return await client.GetStreamAsync(url);
        }

        async public static Task<byte[]> GetByteArrayFromUrlAsync(string url)
        {
            using HttpClient client = new();
            return await client.GetByteArrayAsync(url);
        }

        public static void SetLogger(ILogger<Worker> log)
        {
            logger = log;
        }

        public static async Task<bool> IsValidUrl(string url)
        {
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void LogInformation(string message)
        {
            logger.LogInformation(string.Format("{0}-{1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), message));
        }
        public static void LogError(string message)
        {
            logger.LogError(string.Format("{0}-{1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), message));
        }

        public static string ReturnValidRarityFromCommand(string rarity)
        {
            List<string> validRarity = new();
            rarity = rarity.Trim();
            if (rarity.Contains(","))
            {
                foreach (string partialRarity in rarity.Split(","))
                {
                    var temp = partialRarity.Trim();
                    if (IsValidRarity(temp))
                    {
                        validRarity.Add(temp.ToUpper());
                    }
                }
            }
            else
            {
                if (IsValidRarity(rarity))
                {
                    validRarity.Add(rarity.ToUpper());
                }
            }

            return string.Join(',', validRarity);
        }

        public static bool IsValidRarity(string rarity)
        {
            return new List<string> { "C", "U", "R", "M", "ALL" }.Contains(rarity.ToUpper());
        }
    }
}
