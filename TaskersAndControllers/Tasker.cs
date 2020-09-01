using System.Collections.Generic;
using System.Threading.Tasks;

namespace MagicNewCardsBot
{
    public abstract class Tasker
    {

        #region Public Methods

        async public IAsyncEnumerable<Card> GetNewCards()
        {
            //get the aditional infos from the website
            await foreach (Card card in GetAvaliableCardsInWebSite())
            {
                var cardInDb = await Database.IsCardInDatabase(card, true);
                if (cardInDb == false)
                {
                    await GetAdditionalInfo(card);
                    //adds in the database
                    await Database.InsertScryfallCard(card);

                    yield return card;
                    //fires the event to do stuffs with the new object
                }
            }
        }
        #endregion


        #region Abstract methods
        abstract protected Task GetAdditionalInfo(Card spoil);
        abstract protected IAsyncEnumerable<Card> GetAvaliableCardsInWebSite();
        #endregion
    }
}
