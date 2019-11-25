using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace MagicBot
{

    public class TwitterController
    {
        #region Definitions
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _acessToken;
        private readonly string _acessTokenSecret;
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
        async public Task PublishNewImage(Card card)
        {

            List<IMedia> lstImages = new List<IMedia>();

            //loads the image and sends it

            byte[] byteImage = await Program.GetByteArrayFromUrlAsync(card.ImageUrl);
            IMedia mainImage = Upload.UploadBinary(byteImage);
            lstImages.Add(mainImage);


            if (card.ExtraSides != null && card.ExtraSides.Count > 0)
            {
                foreach (Card extraCard in card.ExtraSides)
                {
                    byte[] extraByteImage = await Program.GetByteArrayFromUrlAsync(extraCard.ImageUrl);
                    IMedia extraImage = Upload.UploadBinary(extraByteImage);
                    lstImages.Add(extraImage);
                }
            }

            await TweetAsync.PublishTweet(card.GetTwitterText(), new Tweetinvi.Parameters.PublishTweetOptionalParameters
            {
                Medias = lstImages
            });
        }
    }
    #endregion

}