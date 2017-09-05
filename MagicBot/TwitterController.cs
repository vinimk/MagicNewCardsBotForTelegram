using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tweetinvi;
using ImageSharp;
using System.IO;
using System.Linq;
using Tweetinvi.Models;

namespace MagicBot
{

    public class TwitterController
    {
        #region Definitions
        private String _consumerKey;
        private String _consumerSecret;
        private String _acessToken;
        private String _acessTokenSecret;
        #endregion

        #region Constructor
        public TwitterController(String consumerKey, string consumerSecret, String acessToken, String acessTokenSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _acessToken = acessToken;
            _acessTokenSecret = acessTokenSecret;
            Auth.SetUserCredentials(_consumerKey, _consumerSecret, _acessToken, _acessTokenSecret);
        }
        #endregion

        #region Public Methods
        public void PublishNewImage(Image<Rgba32> image, String cardName)
        {
            //gets a temp file for the image
            String pathTempImage = System.IO.Path.GetTempFileName();
            //saves the image in the disk in the temp file
            FileStream fileStream = new FileStream(pathTempImage, FileMode.OpenOrCreate);
            image.Save(fileStream, ImageSharp.ImageFormats.Png);
            fileStream.Flush();
            fileStream.Close();

            //loads the image and sends it
            byte[] file1 = File.ReadAllBytes(pathTempImage);
            var media = Upload.UploadImage(file1);

            var tweet = Tweet.PublishTweet(String.Format("New magic card: {0}", cardName), new Tweetinvi.Parameters.PublishTweetOptionalParameters
            {
                Medias = new List<IMedia> { media }
            });

        }
        #endregion

    }
}