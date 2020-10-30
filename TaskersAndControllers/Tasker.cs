using System.Collections.Generic;
using System.Threading.Tasks;

namespace MagicNewCardsBot
{
    public abstract class Tasker
    {

        #region Public Methods

        async public IAsyncEnumerable<Card> GetNewCardsAsync()
        {
            //get the aditional infos from the website
            await foreach (Card card in GetAvaliableCardsInWebSiteAsync())
            {
                var cardInDb = await Database.IsCardInDatabaseAsync(card, true);
                if (cardInDb == false)
                {
                    await GetAdditionalInfoAsync(card);
                    //adds in the database
                    await Database.InsertScryfallCardAsync(card);

                    yield return card;
                }
            }
        }
        #endregion


        #region Abstract methods
        abstract protected Task GetAdditionalInfoAsync(Card card);
        abstract protected IAsyncEnumerable<Card> GetAvaliableCardsInWebSiteAsync();
        #endregion
    }
}
