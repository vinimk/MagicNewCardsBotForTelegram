using HtmlAgilityPack;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MagicNewCardsBot
{
    public class MTGVisualTasker
    {
        public MTGVisualTasker()
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
            await GetAndProcessAvaliableCardsInWebSite();
        }

        async private Task GetAndProcessAvaliableCardsInWebSite()
        {
            var sets = await Database.GetAllCrawlableSets();
            foreach (var set in sets)
            {
                try
                {
                    //loads the website
                    HtmlWeb htmlWeb = new HtmlWeb();
                    HtmlDocument doc = await htmlWeb.LoadFromWebAsync(set.URL);

                    var nodes = doc.DocumentNode.SelectNodes(".//div[@class='spoiler-set-card']");
                    foreach (var node in nodes)
                    {
                        Card card = new Card();
                        card.Name = node.SelectSingleNode(".//a[@rel='bookmark']")?.Attributes["title"]?.Value;
                        card.ImageUrl = node.SelectSingleNode(".//img[contains(@class,'attachment-set-card')]")?.Attributes["src"]?.Value;
                        await CheckCard(card);
                    }

                }


                catch (Exception ex)
                {
                    Database.InsertLog("Error crawling page: " + set.URL, String.Empty, ex.ToString()).Wait();
                    Utils.LogInformation("Error crawling the main page");
                    Utils.LogInformation(ex.Message);
                    Utils.LogInformation(ex.StackTrace);
                }
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
                if (await checkImage(card))
                {
                    //adds in the database
                    await Database.InsertScryfallCard(card);

                    //fires the event to do stuffs with the new object
                    OnNewCard(card);
                }
            }
        }

        async private Task<bool> checkImage(Card card)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    var downloadedData = await webClient.DownloadDataTaskAsync(card.ImageUrl);
                    using (Image<Rgba32> image = Image.Load(downloadedData))
                    {
                        if (image.Width > 100 || image.Height > 100)
                            return true;
                        else
                            return false;
                    }
                }
            }
            catch
            {
                return false;
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
                    card.ImageUrl = WebUtility.HtmlDecode(html.DocumentNode.SelectSingleNode("//img[@class='card-spoiler-image']")?.Attributes["src"]?.Value);
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
                    card.Flavor = WebUtility.HtmlDecode(html.DocumentNode.SelectSingleNode("//div[@class='t-spoiler-flavor']")?.InnerText.Trim());
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