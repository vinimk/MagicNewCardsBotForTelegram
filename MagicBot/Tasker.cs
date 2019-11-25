using System.Collections.Generic;
using System.Threading.Tasks;

namespace MagicBot
{
    public abstract class Tasker
    {
        #region Events
        public delegate Task NewCard(object sender, Card e);
        //public delegate void NewCard(object sender, Card newItem);
        public event NewCard EventNewcard;
        async protected virtual void OnNewCard(Card args)
        {
            if (EventNewcard != null)
                await EventNewcard(this, args);
        }
        #endregion

        #region Public Methods

        async public Task GetNewCards()
        {
            await CheckNewCards();
        }
        #endregion

        #region Private Methods
        async private Task CheckNewCards()
        {
            //get the aditional infos from the website
            List<Card> lstCards = await GetAvaliableCardsInWebSite();
            foreach (Card card in lstCards)
            {
                await CheckCard(card);
            }
        }

        async private Task CheckCard(Card card)
        {
            //check if the spoil is in the database
            //if is not in the database AND has NOT been sent
            var cardInDb = await Database.IsCardInDatabase(card, true);
            if (cardInDb == false)
            {
                card = await GetAdditionalInfo(card);
                //adds in the database
                await Database.InsertScryfallCard(card);

                //fires the event to do stuffs with the new object
                OnNewCard(card);
            }
        }

        #endregion

        #region Abstract methods
        abstract protected Task<Card> GetAdditionalInfo(Card spoil);
        abstract protected Task<List<Card>> GetAvaliableCardsInWebSite();
        #endregion
    }
}
