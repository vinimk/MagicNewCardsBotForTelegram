using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MagicNewCardsBot
{
    public abstract class Tasker
    {
        protected readonly string _websiteUrl;
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
                    if (card.ExtraSides != null && card.ExtraSides.Count > 0)
                    {
                        if (await Database.IsExtraSideInDatabase(card, true) == true)
                            continue;

                        foreach (Card extraSide in card.ExtraSides)
                        {
                            if (!string.IsNullOrEmpty(extraSide.FullUrlWebSite))
                                await Database.InsertScryfallCardAsync(card, true);
                            else
                                extraSide.FullUrlWebSite = card.FullUrlWebSite;
                        }
                    }

                    //adds in the database
                    await Database.InsertScryfallCardAsync(card);

                    yield return card;
                }
            }
        }

        protected static async Task<HtmlDocument> GetHtmlDocumentFromUrlAsync(string url)
        {
            HtmlDocument html = new();
            //crawl the webpage to get this information
            using Stream stream = await Utils.GetStreamFromUrlAsync(url);
            html.Load(stream, Encoding.GetEncoding("ISO-8859-1"));
            return html;
        }
        #endregion


        #region Abstract methods
        abstract protected Task GetAdditionalInfoAsync(Card card);
        abstract protected IAsyncEnumerable<Card> GetAvaliableCardsInWebSiteAsync();
        #endregion
    }
}
