using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
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
        public void PublishNewImage(Card card)
        {

            List<IMedia> lstImages = new List<IMedia>();

            //loads the image and sends it
            using(System.IO.Stream imageStream = Program.GetImageFromUrl(card.ImageUrl))
            {
                byte[] byteImage = Program.ReadFully(imageStream);
                IMedia mainImage = Upload.UploadBinary(byteImage);
                lstImages.Add(mainImage);
            }

            if (card.ExtraSides != null && card.ExtraSides.Count > 0)
            {
                foreach (Card extraCard in card.ExtraSides)
                {
                    using(System.IO.Stream extraImageStream = Program.GetImageFromUrl(extraCard.ImageUrl))
                    {
                        byte[] extraByteImage = Program.ReadFully(extraImageStream);
                        IMedia extraImage = Upload.UploadBinary(extraByteImage);
                        lstImages.Add(extraImage);
                    }
                }
            }

            var tweet = Tweet.PublishTweet(card.GetTwitterText(), new Tweetinvi.Parameters.PublishTweetOptionalParameters
            {
                Medias = lstImages
            });
        }
    }
    #endregion

}