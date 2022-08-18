using MagicNewCardsBot.Helpers;
using MagicNewCardsBot.StorageClasses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Logic.QueryParameters;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace MagicNewCardsBot.TaskersAndControllers
{

    public class TwitterController
    {
        #region Definitions
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _acessToken;
        private readonly string _acessTokenSecret;
        private readonly TwitterClient _twitterClient;
        #endregion

        #region Constructor
        public TwitterController(string consumerKey, string consumerSecret, string acessToken, string acessTokenSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _acessToken = acessToken;
            _acessTokenSecret = acessTokenSecret;

            _twitterClient = new TwitterClient(_consumerKey, _consumerSecret, _acessToken, _acessTokenSecret);

        }
        #endregion

        #region Public Methods
        async public Task TweetCardAsync(Card card)
        {

            List<IMedia> lstImages = new();

            //loads the image and sends it

            byte[] byteImage = await Utils.GetByteArrayFromUrlAsync(card.ImageUrl);
            IMedia mainImage = await _twitterClient.Upload.UploadTweetImageAsync(byteImage);
            await _twitterClient.Upload.AddMediaMetadataAsync(new MediaMetadata(mainImage)
            {
                AltText = card.GetTwitterAltText()
            });

            lstImages.Add(mainImage);


            if (card.ExtraSides != null && card.ExtraSides.Count > 0)
            {
                foreach (Card extraCard in card.ExtraSides)
                {
                    byte[] extraByteImage = await Utils.GetByteArrayFromUrlAsync(extraCard.ImageUrl);
                    IMedia extraImage = await _twitterClient.Upload.UploadTweetImageAsync(extraByteImage);

                    await _twitterClient.Upload.AddMediaMetadataAsync(new MediaMetadata(extraImage)
                    {
                        AltText = extraCard.GetTwitterAltText()
                    });

                    lstImages.Add(extraImage);
                }
            }

            await _twitterClient.Tweets.PublishTweetAsync(new PublishTweetParameters
            {
                Medias = lstImages,
                Text = card.GetTwitterText()
            });
        }
    }

    #endregion

}