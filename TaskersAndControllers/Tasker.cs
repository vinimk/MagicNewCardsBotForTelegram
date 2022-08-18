﻿using HtmlAgilityPack;
using MagicNewCardsBot.Helpers;
using MagicNewCardsBot.StorageClasses;
using System.Text;
using static MagicNewCardsBot.Helpers.Database;

namespace MagicNewCardsBot.TaskersAndControllers
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
                var cardInDb = await GetCardStatus(card);
                if (cardInDb != CardStatus.Complete)
                {
                    await GetAdditionalInfoAsync(card);
                    if (!string.IsNullOrEmpty(card.ImageUrl))
                    {
                        if (card.ExtraSides != null && card.ExtraSides.Count > 0)
                        {
                            if (await IsExtraSideInDatabase(card, true) == true)
                            {
                                bool flagContinue = false;
                                await InsertScryfallCardAsync(card, true, card.Rarity.HasValue);
                                foreach (var extraSide in card.ExtraSides)
                                {
                                    if (string.IsNullOrEmpty(extraSide.FullUrlWebSite))
                                    {
                                        extraSide.FullUrlWebSite = card.FullUrlWebSite;
                                    }

                                    var statusExtraSide = await GetCardStatus(extraSide);

                                    switch (statusExtraSide)
                                    {
                                        case CardStatus.Complete:
                                            flagContinue = true;
                                            continue;
                                        case CardStatus.NotFound:
                                            await InsertScryfallCardAsync(extraSide, true, extraSide.Rarity.HasValue);
                                            break;
                                        case CardStatus.WithoutRarity:
                                            //do nothing
                                            break;
                                    }
                                }

                                if (flagContinue)
                                {
                                    continue;
                                }
                            }
                        }

                        //adds in the database
                        if (cardInDb == CardStatus.NotFound)
                        {
                            await InsertScryfallCardAsync(card, false, card.Rarity.HasValue);
                            if (card.Rarity.HasValue)
                            {
                                card.SendTo = SendTo.Both;
                            }
                            else
                            {
                                card.SendTo = SendTo.OnlyAll;
                            }
                        }
                        else if (cardInDb == CardStatus.WithoutRarity && card.Rarity.HasValue)
                        {
                            await UpdateHasRarityAsync(card, true);
                            card.SendTo = SendTo.OnlyRarity;
                        }
                        else
                        {
                            continue;
                        }


                        yield return card;
                    }
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
