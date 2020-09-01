using HtmlAgilityPack;
using MagicNewCardsBot.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MagicNewCardsBot
{
    public class MTGPicsTasker : Tasker
    {
        #region Definitions
        private readonly string _websiteUrl = "https://www.mtgpics.com/";
        #endregion

        #region Overrided Methods

        async override protected IAsyncEnumerable<Card> GetAvaliableCardsInWebSite()
        {
            List<Set> setsToCrawl = await Database.GetAllCrawlableSets();
            foreach (Set set in setsToCrawl)
            {
                //loads the website

                HtmlDocument doc = new HtmlDocument();
                //crawl the webpage to get this information
                using (Stream stream = await Utils.GetStreamFromUrlAsync(set.URL))
                {
                    doc.Load(stream);
                }

                //all the cards are a a href so we get all of that
                HtmlNodeCollection nodesCards = doc.DocumentNode.SelectNodes("//td[@valign='middle']");
                if (nodesCards != null)
                {
                    int crawlsFromThisSite = 0;
                    foreach (HtmlNode node in nodesCards)
                    {
                        var nodeImageCard = node.SelectSingleNode(".//img");
                        //also the cards have a special class called 'card', so we use it to get the right ones
                        if (nodeImageCard != null &&
                            nodeImageCard.Attributes.Contains("src") &&
                            nodeImageCard.Attributes["src"].Value.ToString().EndsWith(".jpg"))
                        {
                            string cardUrl = nodeImageCard.ParentNode.Attributes["href"].Value.ToString();
                            if (!cardUrl.Contains(_websiteUrl))
                            {
                                cardUrl = _websiteUrl + cardUrl;
                            }

                            string imageUrl = nodeImageCard.Attributes["src"].Value.ToString();
                            if (!imageUrl.Contains(_websiteUrl))
                            {
                                imageUrl = _websiteUrl + imageUrl;
                            }
                            var card = new Card
                            {
                                FullUrlWebSite = cardUrl,
                                ImageUrl = imageUrl
                            };
                            yield return card;
                            crawlsFromThisSite++;
                        }

                        //only get the lastest 50
                        if (crawlsFromThisSite == Database.MAX_CARDS)
                            break;
                    }
                }
            }
        }

        private void processFieldByType(IList<HtmlNode> nodes, Card mainCard, CardFields field)
        {

            for (int i = 0; i < nodes.Count; i++)
            {
                try
                {
                    var node = nodes[i];
                    Card card;
                    if (i == 0)
                        card = mainCard;
                    else
                        card = mainCard.ExtraSides[i - 1];

                    switch (field)
                    {
                        case CardFields.Name:
                            card.Name = node.InnerText.Trim();
                            break;

                        case CardFields.ManaCost:
                            string totalCost = String.Empty;
                            foreach (var childNode in node.ChildNodes)
                            {
                                string imgUrl = childNode.Attributes["src"].Value;
                                if (imgUrl.EndsWith(".png"))
                                {
                                    int lastIndex = imgUrl.LastIndexOf('/');
                                    string cost = imgUrl.Substring(lastIndex + 1);
                                    cost = cost.Replace(".png", String.Empty);
                                    totalCost += cost.Trim().ToUpper();
                                }
                            }
                            if (!String.IsNullOrEmpty(totalCost))
                            {
                                card.ManaCost = totalCost;
                            }
                            break;
                        case CardFields.Type:
                            string type = node.InnerText;
                            type = type.Replace("\u0097", "-");
                            type = System.Net.WebUtility.HtmlDecode(type);
                            type = type.Trim();

                            if (!String.IsNullOrEmpty(type))
                                card.Type = type;

                            break;
                        case CardFields.Text:
                            StringBuilder sb = new StringBuilder();
                            foreach (var childNode in node.ChildNodes)
                            {
                                if (childNode.Attributes != null)
                                {
                                    if (childNode.Attributes["alt"] != null)
                                    {
                                        string symbol = childNode.Attributes["alt"].Value;
                                        symbol = symbol.Replace("%", "");
                                        sb.Append(symbol.ToUpper());
                                        continue;
                                    }
                                    else if (childNode.Attributes["onclick"] != null && childNode.Attributes["onclick"].Value.Contains("LoadGlo"))
                                    {
                                        continue;
                                    }
                                }

                                string text = childNode.InnerText.Replace("\u0095", "•");
                                text = text.Replace("\u0097", "-");
                                text = text.Replace("\n\t\t\t", String.Empty);
                                text = text.Replace("  ", " ");
                                text = WebUtility.HtmlDecode(text);

                                sb.Append(text);
                            }

                            string sbString = sb.ToString();
                            if (!String.IsNullOrEmpty(sbString))
                                card.Text = sbString;

                            break;
                        case CardFields.PT:
                            foreach (var childNodes in node.ChildNodes)
                            {
                                var text = childNodes.InnerText.Trim();
                                text = text.Replace("\n", String.Empty);
                                if (text.Contains("/"))
                                {
                                    String[] arrPt = text.Split('/');
                                    if (arrPt.Length == 2)
                                    {
                                        card.Power = arrPt[0];
                                        card.Toughness = arrPt[1];
                                        break;
                                    }
                                }
                                else if (text.Contains("Loyalty:") || text.Contains("Loyalty :"))
                                {
                                    string numbersOnly = Regex.Replace(text, "[^0-9]", "");
                                    card.Loyalty = Convert.ToInt32(numbersOnly);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch { }
            }
        }

        async override protected Task GetAdditionalInfo(Card card)
        {
            //we do all of this in empty try catches because it is not mandatory information
            try
            {
                HtmlDocument html = new HtmlDocument();
                //crawl the webpage to get this information
                using (Stream stream = await Utils.GetStreamFromUrlAsync(card.FullUrlWebSite))
                {
                    html.Load(stream, Encoding.GetEncoding("ISO-8859-1"));
                }

                //IMAGE
                try
                {
                    var nodeParent = html.DocumentNode.SelectSingleNode(".//div[@id='CardScan']");
                    var nodeImage = nodeParent.SelectSingleNode(".//img");
                    string urlNewImage = nodeImage.Attributes["src"].Value.ToString();
                    if (urlNewImage.Contains(_websiteUrl))
                    {
                        card.ImageUrl = urlNewImage;
                    }
                }
                catch
                { }


                //SEE IF IT HAS EXTRA IMAGES
                try
                {
                    var nodeParent = html.DocumentNode.SelectSingleNode(".//div[@id='CardScanBack']");
                    if (nodeParent != null)
                    {

                        var nodeImage = nodeParent.SelectSingleNode(".//img");
                        string urlNewImage = nodeImage?.Attributes["src"].Value.ToString();
                        if (!urlNewImage.Contains(_websiteUrl))
                        {
                            urlNewImage = _websiteUrl + urlNewImage;
                        }

                        if (!string.IsNullOrEmpty(urlNewImage))
                        {
                            if (card.ExtraSides == null)
                                card.ExtraSides = new List<Card>();

                            card.ExtraSides.Add(new Card() { ImageUrl = urlNewImage });

                        }
                    }
                }
                catch
                { }


                //NAME
                var nodes = html.DocumentNode.SelectNodes(".//div[@class='Card20']");

                if (nodes != null)
                    processFieldByType(nodes, card, CardFields.Name);

                //MANA COST
                nodes = html.DocumentNode.SelectNodes(".//div[@style='height:25px;float:right;']");

                if (nodes != null)
                    processFieldByType(nodes, card, CardFields.ManaCost);

                //TEXT
                nodes = html.DocumentNode.SelectNodes(".//div[@id='EngShort']");
                if (nodes != null)
                    processFieldByType(nodes, card, CardFields.Text);

                var g16nodes = html.DocumentNode.SelectNodes(".//div[@class='CardG16']");

                //TYPE 
                var nodesType = g16nodes.Where(x => x.Attributes["style"] != null  &&
                                                    x.Attributes["style"].Value.Trim().Equals("padding:5px 0px 5px 0px;") 
                                                ).ToList();

                if (nodesType != null)
                    processFieldByType(nodesType, card, CardFields.Type);

                //POWER AND TOUGHNESS AND LOYALTY
                var nodesPT = g16nodes.Where(x => x.Attributes["align"] != null &&
                                                !String.IsNullOrEmpty(x.InnerText.Trim()) &&
                                                x.Attributes["align"].Value.Trim().Equals("right")).ToList();

                if (nodesPT != null)
                    processFieldByType(nodesPT, card, CardFields.PT);

            }
            catch
            { }
        }

        #endregion
    }
}