using System;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using System.Text.RegularExpressions;

namespace MagicBot
{
    public class MTGSalvationTasker
    {
        internal readonly String _apiUrl = "https://www.mtgsalvation.com/spoilers.rss";
        public MTGSalvationTasker()
        { }

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
            for (int i = 0; i < Database.MAX_CARDS; i++)
            {
                if (i < lstCards.Count)
                {
                    Card card = lstCards[i];
                    await CheckCard(card);
                }
            }
        }

        async private Task<List<Card>> GetAvaliableCardsInWebSite()
        {
            var cards = new List<Card>();
            var feed = await FeedReader.ReadAsync(this._apiUrl);

            foreach (var item in feed.Items)
            {
                var withoutBr = item.Description.Replace("<br>", string.Empty).Replace("Rules Text:", "Text:");
                string plainText = HtmlToText.ConvertHtml(withoutBr);
                string content = Regex.Replace(plainText, @"\r\n?|\n", " ");

                var kvps = ParsePrinterResponse(content);

                var name = kvps.Where(x => x.Key == "Name").Select(x => x.Value).FirstOrDefault();
                var cost = kvps.Where(x => x.Key == "Cost").Select(x => x.Value).FirstOrDefault();
                var type = kvps.Where(x => x.Key == "Type").Select(x => x.Value).FirstOrDefault();
                var text = kvps.Where(x => x.Key == "Text").Select(x => x.Value).FirstOrDefault();
                var set = kvps.Where(x => x.Key == "Set").Select(x => x.Value).FirstOrDefault();


                cards.Add(new Card
                {
                    FullUrlWebSite = item.Link,
                    Name = name,
                    ManaCost = cost,
                    Type = type,
                    Text = text,
                    Set = set,
                });
            }

            return cards;
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


        async private Task<Card> GetAdditionalInfo(Card card)
        {
            //we do all of this in empty try catches because it is not mandatory information
            try
            {
                //crawl the webpage to get this information
                HtmlWeb htmlWeb = new HtmlWeb();
                HtmlDocument html = await htmlWeb.LoadFromWebAsync(card.FullUrlWebSite);

                try
                {
                    card.ImageUrl = WebUtility.HtmlDecode(html.DocumentNode.SelectSingleNode("//meta[@property='og:image']")?.Attributes["content"]?.Value);
                }
                catch
                { }

                try
                {
                    card.Name = WebUtility.HtmlDecode(html.DocumentNode.SelectSingleNode("//h2[@class='caption']")?.InnerText);
                }
                catch
                { }

                try
                {
                    card.Text = WebUtility.HtmlDecode(html.DocumentNode.SelectSingleNode("//meta[@name='description']")?.Attributes["content"]?.Value);
                }
                catch
                { }

                try
                {
                    card.Type = WebUtility.HtmlDecode(html.DocumentNode.SelectSingleNode("//span[@class='t-spoiler-type j-search-html']")?.InnerText);
                }
                catch
                { }

                try
                {
                    card.Flavor = WebUtility.HtmlDecode(html.DocumentNode.SelectSingleNode("//div[@class='t-spoiler-flavor']")?.InnerText).Trim();
                }
                catch
                { }
            }
            catch
            { }
            return card;
        }


        private List<KeyValuePair<string, string>> ParsePrinterResponse(string rawResponse)
        {
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            string[] colonItems = rawResponse.Trim().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (colonItems.Length > 1)
            {
                string currentKey = colonItems[0], currentValue = "";
                for (int i = 1; i < colonItems.Length; i++)
                {
                    string currentItem = colonItems[i];
                    int spaceIndex = currentItem.LastIndexOf(" ");
                    if (spaceIndex < 0)
                    {
                        //end of string, whole item is the value
                        currentValue = currentItem;
                    }
                    else
                    {
                        //middle of string, left part is value, right part is next key
                        currentValue = currentItem.Substring(0, spaceIndex);
                    }
                    pairs.Add(new KeyValuePair<string, string>(currentKey, currentValue));
                    currentKey = currentItem.Substring(spaceIndex + 1);
                }
            }
            return pairs;
        }


        #endregion

        #region Events
        public delegate void NewCard(object sender, Card newItem);
        public event NewCard eventNewcard;
        protected virtual void OnNewCard(Card args)
        {
            if (eventNewcard != null)
                eventNewcard(this, args);
        }
        #endregion
    }
}