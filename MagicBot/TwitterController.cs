using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tweetinvi;
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
        public void PublishNewImage(ScryfallCard card)
        {

            List<IMedia> lstImages = new List<IMedia>();

            //loads the image and sends it
            using (System.IO.Stream imageStream = Program.GetImageFromUrl(card.image_url))
            {
                byte[] byteImage = Program.ReadFully(imageStream);
                IMedia mainImage = Upload.UploadImage(byteImage);
                lstImages.Add(mainImage);
            }

            if (card.ExtraSides != null)
            {
                foreach (ScryfallCard extraCard in card.ExtraSides)
                {
                    using (System.IO.Stream extraImageStream = Program.GetImageFromUrl(extraCard.image_url))
                    {
                        byte[] extraByteImage = Program.ReadFully(extraImageStream);
                        IMedia extraImage = Upload.UploadImage(extraByteImage);
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
