using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tweetinvi;
using SixLabors.ImageSharp;
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
        public void PublishNewImage(SpoilItem spoil)
        {
            List<IMedia> lstImages = new List<IMedia>();

            //gets a temp file for the image
            String pathTempImage = System.IO.Path.GetTempFileName();
            //saves the image in the disk in the temp file
            FileStream fileStream = new FileStream(pathTempImage, FileMode.OpenOrCreate);
            spoil.Image.Save(fileStream, ImageFormats.Png);
            fileStream.Flush();
            fileStream.Close();

            //loads the image and sends it
            byte[] fileBits = File.ReadAllBytes(pathTempImage);
            IMedia mainImage = Upload.UploadImage(fileBits);
            lstImages.Add(mainImage);

            if (spoil.AdditionalImage != null)
            {
                //gets a temp file for the image
                String pathTempImageAdditional = System.IO.Path.GetTempFileName();
                //saves the image in the disk in the temp file
                FileStream fileStreamAdditional = new FileStream(pathTempImageAdditional, FileMode.OpenOrCreate);
                spoil.AdditionalImage.Save(fileStreamAdditional, ImageFormats.Png);
                fileStreamAdditional.Flush();
                fileStreamAdditional.Close();

                //loads the image and sends it
                byte[] fileBitsAdditional = File.ReadAllBytes(pathTempImageAdditional);
                IMedia additionalImage = Upload.UploadImage(fileBitsAdditional);
                lstImages.Add(additionalImage);
            }

            var tweet = Tweet.PublishTweet(spoil.GetTwitterText(), new Tweetinvi.Parameters.PublishTweetOptionalParameters
            {
                Medias = lstImages
            });

        }
        #endregion

    }
}