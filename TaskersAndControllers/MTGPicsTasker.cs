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
        protected new string _websiteUrl = "https://www.mtgpics.com/";
        #endregion

        protected string ValidateAndFixUrl(string url)
        {
            if (!url.Contains(_websiteUrl))
            {
                url = _websiteUrl + url;
            }
            return url;
        }

        #region Overrided Methods

        async override protected IAsyncEnumerable<Card> GetAvaliableCardsInWebSiteAsync()
        {
            List<Set> setsToCrawl = await Database.GetAllCrawlableSetsAsync();
            foreach (Set set in setsToCrawl)
            {
                //loads the website

                HtmlDocument doc = new();
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
                            cardUrl = ValidateAndFixUrl(cardUrl);


                            string imageUrl = nodeImageCard.Attributes["src"].Value.ToString();
                            imageUrl = ValidateAndFixUrl(imageUrl);

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

        private static void ProcessFieldByType(IList<HtmlNode> nodes, Card mainCard, CardFields field)
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
                                    string cost = imgUrl[(lastIndex + 1)..];
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
                            StringBuilder sb = new();
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

        private string GetImageUrlFromHtmlDoc(HtmlDocument html)
        {
            try
            {
                var nodeParent = html.DocumentNode.SelectSingleNode(".//div[@id='CardScan']");
                var nodeImage = nodeParent.SelectSingleNode(".//img");
                string urlNewImage = nodeImage.Attributes["src"].Value.ToString();
                urlNewImage = ValidateAndFixUrl(urlNewImage);
                return urlNewImage;
            }
            catch
            { }
            return null;
        }

        private static void GetDetails(Card card, HtmlDocument html)
        {

            //NAME
            var nodes = html.DocumentNode.SelectNodes(".//div[@class='Card20']");

            if (nodes != null)
                ProcessFieldByType(nodes, card, CardFields.Name);

            //MANA COST
            nodes = html.DocumentNode.SelectNodes(".//div[@style='height:25px;float:right;']");

            if (nodes != null)
                ProcessFieldByType(nodes, card, CardFields.ManaCost);

            //TEXT
            nodes = html.DocumentNode.SelectNodes(".//div[@id='EngShort']");
            if (nodes != null)
                ProcessFieldByType(nodes, card, CardFields.Text);

            var g16nodes = html.DocumentNode.SelectNodes(".//div[@class='CardG16']");

            //TYPE 
            var nodesType = g16nodes.Where(x => x.Attributes["style"] != null &&
                                                x.Attributes["style"].Value.Trim().Equals("padding:5px 0px 5px 0px;")
                                            ).ToList();

            if (nodesType != null)
                ProcessFieldByType(nodesType, card, CardFields.Type);

            //POWER AND TOUGHNESS AND LOYALTY
            var nodesPT = g16nodes.Where(x => x.Attributes["align"] != null &&
                                            !String.IsNullOrEmpty(x.InnerText.Trim()) &&
                                            x.Attributes["align"].Value.Trim().Equals("right")).ToList();

            if (nodesPT != null)
                ProcessFieldByType(nodesPT, card, CardFields.PT);

        }

        private void GetExtraSides(Card card, HtmlDocument html)
        {
            //SEE IF IT HAS EXTRA IMAGES
            try
            {
                var nodeParent = html.DocumentNode.SelectSingleNode(".//div[@id='CardScanBack']");
                if (nodeParent != null)
                {
                    var nodeImage = nodeParent.SelectSingleNode(".//img");
                    string urlNewImage = nodeImage?.Attributes["src"].Value.ToString();
                    urlNewImage = ValidateAndFixUrl(urlNewImage);

                    if (!string.IsNullOrEmpty(urlNewImage))
                    {
                        card.AddExtraSide(new Card() { ImageUrl = urlNewImage });
                    }
                }
            }
            catch
            { }
        }

        async override protected Task GetAdditionalInfoAsync(Card card)
        {
            //we do all of this in empty try catches because it is not mandatory information
            try
            {
                HtmlDocument html = await GetHtmlDocumentFromUrlAsync(card.FullUrlWebSite);

                //IMAGE
                card.ImageUrl = GetImageUrlFromHtmlDoc(html);

                GetExtraSides(card, html);
                GetDetails(card, html);


                //SEE IF IT HAS ALTERNATIVE ARTS/STYLE CARDS
                try
                {
                    for (int i = 1; i < 10; i++)
                    {
                        var resultNodes = html.DocumentNode.SelectNodes($"//text()[. = '{i}']");
                        if (resultNodes != null)
                        {
                            foreach (HtmlNode node in resultNodes)
                            {
                                var nodeParent = node.ParentNode;
                                if (nodeParent != null && nodeParent.Attributes.Count > 0 && nodeParent.Attributes["href"] != null)
                                {
                                    var hrefAlternative = nodeParent.Attributes["href"].Value;
                                    hrefAlternative = ValidateAndFixUrl(hrefAlternative);

                                    var alternativeHtmlDoc = await GetHtmlDocumentFromUrlAsync(hrefAlternative);

                                    var alernativeImage = GetImageUrlFromHtmlDoc(alternativeHtmlDoc);

                                    if (!String.IsNullOrEmpty(alernativeImage))
                                    {
                                        Card alternativeCard = new() { FullUrlWebSite = hrefAlternative, ImageUrl = alernativeImage };
                                        GetExtraSides(alternativeCard, alternativeHtmlDoc);
                                        GetDetails(alternativeCard, alternativeHtmlDoc);

                                        //small workarround to not cascade extra sides
                                        card.AddExtraSide(alternativeCard);

                                        for (int j = 0; j < alternativeCard.ExtraSides.Count; j++)
                                        {
                                            Card extraSideAlternative = alternativeCard.ExtraSides[j];
                                            card.AddExtraSide(extraSideAlternative);
                                            alternativeCard.ExtraSides.RemoveAt(j);
                                        }

                                    }
                                }
                            }
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