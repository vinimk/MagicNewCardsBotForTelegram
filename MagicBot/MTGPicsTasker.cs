using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MagicBot
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
                using (Stream stream = await Program.GetStreamFromUrlAsync(set.URL))
                {
                    doc.Load(stream);
                }

                //all the cards are a a href so we get all of that
                HtmlNodeCollection nodesCards = doc.DocumentNode.SelectNodes("//div[@class='S12']");
                if (nodesCards != null)
                {
                    int crawlsFromThisSite = 0;
                    foreach (HtmlNode node in nodesCards)
                    {
                        var nodeImageCard = node.SelectSingleNode(".//img");
                        //also the cards have a special class called 'card', so we use it to get the right ones
                        if (nodeImageCard.Attributes.Contains("src") &&
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
                        }

                        crawlsFromThisSite++;
                        //only get the lastest 50
                        if (crawlsFromThisSite == Database.MAX_CARDS)
                            break;
                    }
                }
            }
        }


        async override protected Task GetAdditionalInfo(Card spoil)
        {
            //we do all of this in empty try catches because it is not mandatory information
            try
            {
                HtmlDocument html = new HtmlDocument();
                //crawl the webpage to get this information
                using (Stream stream = await Program.GetStreamFromUrlAsync(spoil.FullUrlWebSite))
                {
                    html.Load(stream, Encoding.GetEncoding("ISO-8859-1"));
                }

                try
                {
                    var nodeParent = html.DocumentNode.SelectSingleNode(".//div[@id='CardScan']");
                    var nodeImage = nodeParent.SelectSingleNode(".//img");
                    string urlNewImage = nodeImage.Attributes["src"].Value.ToString();
                    if (urlNewImage.Contains(_websiteUrl))
                    {
                        spoil.ImageUrl = urlNewImage;
                    }
                }
                catch
                { }

                try
                {
                    var node = html.DocumentNode.SelectSingleNode(".//div[@class='Card20']");
                    var name = node.InnerText.Trim();
                    spoil.Name = name;

                }
                catch
                { }
                try
                {
                    string totalCost = String.Empty;
                    var parentNode = html.DocumentNode.SelectSingleNode(".//div[@style='height:25px;float:right;']");
                    foreach (var node in parentNode.ChildNodes)
                    {

                        string imgUrl = node.Attributes["src"].Value;
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
                        spoil.ManaCost = totalCost;
                    }

                }
                catch
                { }


                try
                {
                    var node = html.DocumentNode.SelectSingleNode(".//div[@class='CardG16']");
                    string type = node.InnerText;
                    type = type.Replace("\u0097", "-");
                    type = System.Net.WebUtility.HtmlDecode(type);
                    type = type.Trim();
                    spoil.Type = type;
                }
                catch
                { }


                try
                {
                    var parentNode = html.DocumentNode.SelectSingleNode(".//div[@id='EngShort']");
                    StringBuilder sb = new StringBuilder();
                    foreach (var node in parentNode.ChildNodes)
                    {
                        if (node.Attributes != null && node.Attributes["alt"] != null)
                        {
                            string symbol = node.Attributes["alt"].Value;
                            symbol = symbol.Replace("%", "");
                            sb.Append(symbol.ToUpper());
                        }
                        else
                        {
                            string text = node.InnerText.Replace("\u0095", "•");
                            text = text.Replace("\u0097", "-");
                            text = text.Replace("\n\t\t\t", String.Empty);
                            text = text.Replace("  ", " ");
                            text = WebUtility.HtmlDecode(text);

                            sb.Append(text);
                        }
                    }
                    spoil.Text = sb.ToString();
                }
                catch
                { }
                try
                {
                    //spoil.Flavor = System.Net.WebUtility.HtmlDecode(html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[5]/td[1]/i[1]").LastChild.InnerText.Trim());
                }
                catch
                { }

                try
                {
                    var nodes = html.DocumentNode.SelectNodes(".//div[@class='CardG16']");

                    foreach (var node in nodes)
                    {
                        var text = node.InnerText.Trim();
                        text = text.Replace("\n", String.Empty);
                        if (text.Contains("/"))
                        {
                            String[] arrPt = text.Split('/');
                            if (arrPt.Length == 2)
                            {
                                spoil.Power = arrPt[0];
                                spoil.Toughness = arrPt[1];
                                break;
                            }
                        }
                        else if (text.Contains("Loyalty:") || text.Contains("Loyalty :"))
                        {
                            string numbersOnly = Regex.Replace(text, "[^0-9]", "");
                            spoil.Loyalty = Convert.ToInt32(numbersOnly);
                        }
                    }

                }
                catch
                { }
            }
            catch
            { }
        }

        #endregion
    }
}