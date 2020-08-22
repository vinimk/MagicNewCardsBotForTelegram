using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MagicNewCardsBot
{
    public static class Utils
    {
        private static ILogger<Worker> logger;
        async public static Task<Stream> GetStreamFromUrlAsync(string url)
        {
            //do a webrequest to get the image
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStreamAsync(url);
            }
        }

        async public static Task<byte[]> GetByteArrayFromUrlAsync(String url)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetByteArrayAsync(url);
            }
        }

        public static void SetLogger(ILogger<Worker> log)
        {
            logger = log;
        }

        public static void LogInformation(String message)
        {
            logger.LogInformation(String.Format("{0}-{1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), message));
        }
        public static void LogError(String message)
        {
            logger.LogError(String.Format("{0}-{1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), message));
        }
    }
}
