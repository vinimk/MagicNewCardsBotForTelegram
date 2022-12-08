using HtmlAgilityPack;
using MagicNewCardsBot.Helpers;
using MagicNewCardsBot.StorageClasses;
using System.Text;

namespace MagicNewCardsBot.TaskersAndControllers
{
    public class MythicApiTask : Tasker
    {
        #region Definitions
        protected new string _websiteUrl = "https://mythicspoiler.com/";
        protected string _page = "newspoilers.html";
        #endregion

        protected string ValidateAndFixUrl(string url)
        {
            if (!url.Contains(_websiteUrl))
            {
                url = _websiteUrl + url;
            }
            return url;
        }

        protected static bool IsSameCard(string url1, string url2)
        {
            string endingUrl1 = url1[(url1.LastIndexOf('/') + 1)..];
            string endingUrl2 = url2[(url2.LastIndexOf('/') + 1)..];
            return endingUrl1 == endingUrl2;
        }

        #region Overrided Methods

        protected override async IAsyncEnumerable<Card> GetAvaliableCardsInWebSiteAsync()
        {
            //loads the website

            HtmlDocument doc = await GetHtmlDocumentFromUrlAsync(_websiteUrl + _page);
            List<string> lstAlreadyReturnedCards = new();
            //all the cards are a a href so we get all of that
            HtmlNodeCollection nodesGridCards = doc.DocumentNode.SelectNodes("//div[contains(@class, 'grid-card')]");
            if (nodesGridCards != null)
            {
                int crawlsFromThisSite = 0;
                foreach (HtmlNode nodeGrid in nodesGridCards)
                {
                    HtmlNode nodeImg = nodeGrid.SelectSingleNode(".//img");
                    if (nodeImg.Attributes.Contains("src") &&
                        nodeImg.Attributes["src"].Value.ToString().Contains("cards") &&
                        nodeImg.Attributes["src"].Value.ToString().Trim().EndsWith(".jpg"))
                    {
                        string cardUrl = nodeImg.ParentNode.Attributes["href"].Value.Trim().ToString();
                        cardUrl = ValidateAndFixUrl(cardUrl);


                        if (lstAlreadyReturnedCards.Contains(cardUrl))
                        {
                            continue;
                        }

                        string imageUrl = nodeImg.Attributes["src"].Value.Trim().ToString();
                        imageUrl = ValidateAndFixUrl(imageUrl);

                        Card card = new()
                        {
                            FullUrlWebSite = cardUrl,
                            ImageUrl = imageUrl
                        };

                        try
                        {
                            HtmlNode nodeCenterCredits = nodeGrid?.SelectSingleNode(".//center");
                            HtmlNode nodeAHrefCredits = nodeCenterCredits?.ParentNode;
                            card.CreditsUrl = nodeAHrefCredits?.Attributes["href"].Value.Trim();
                            HtmlNode nodeTextCredits = nodeCenterCredits?.SelectSingleNode(".//font");
                            card.Credits = nodeTextCredits?.InnerText.ToString().Trim();
                        }
                        catch { }


                        lstAlreadyReturnedCards.Add(card.FullUrlWebSite);
                        yield return card;
                        crawlsFromThisSite++;
                    }

                    //only get the lastest
                    if (crawlsFromThisSite == Database.MAX_CARDS)
                    {
                        break;
                    }
                }
            }

        }

        protected override async Task GetAdditionalInfoAsync(Card card)
        {
            //we do all of this in empty try catches because it is not mandatory information
            try
            {
                //we do all of this in empty try catches because it is not mandatory information
                try
                {
                    HtmlDocument html = await GetHtmlDocumentFromUrlAsync(card.FullUrlWebSite);

                    try
                    {
                        string alternativeSideImageUrl = html.DocumentNode.SelectSingleNode("//comment()[contains(., 'DFC beaxface')]").ParentNode.SelectSingleNode(".//img").Attributes["src"].Value.Trim();
                        alternativeSideImageUrl = ValidateAndFixUrl(alternativeSideImageUrl);
                        alternativeSideImageUrl = alternativeSideImageUrl.Replace("../", string.Empty);
                        if (!IsSameCard(alternativeSideImageUrl, card.ImageUrl))
                        {
                            if (await Utils.IsValidUrl(alternativeSideImageUrl))
                            {
                                card.AddExtraSide(new Card() { ImageUrl = alternativeSideImageUrl });
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        HtmlNode nodeScript = html.DocumentNode.SelectSingleNode("//script[contains(text(),'/cards/')]");
                        if (nodeScript != null)
                        {
                            foreach (string line in nodeScript.InnerHtml.Split("\n"))
                            {
                                if (line.Contains("/cards/"))
                                {
                                    foreach (string part in line.Split("\""))
                                    {
                                        if ((part.Contains("https") || part.Contains("http")) &&
                                            !part.Contains("zzzzzzzzz") &&
                                            !part.Contains("XXXXXXXX"))
                                        {
                                            string alterImageUrl = part.Trim();

                                            if (!IsSameCard(alterImageUrl, card.ImageUrl))
                                            {
                                                if (await Utils.IsValidUrl(alterImageUrl))
                                                {
                                                    card.AddExtraSide(new Card() { ImageUrl = alterImageUrl });
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //foreach (HtmlNode possibleImage in node2.ParentNode.ChildNodes)
                            //{
                            //    if (possibleImage.Attributes["src"] != null)
                            //    {
                            //        Uri url = new Uri(card.FullUrlWebSite);
                            //        string possible = url.AbsoluteUri.Remove(url.AbsoluteUri.Length - url.Segments.Last().Length) + possibleImage.Attributes["src"].Value.ToString();
                            //        if (possible != card.ImageUrl)
                            //        {
                            //            Card extraSide = new Card
                            //            {
                            //                ImageUrl = possible
                            //            };
                            //            card.ExtraSides.Add(extraSide);
                            //        }
                            //    }
                            //}
                        }
                    }
                    catch
                    { }

                    try
                    {
                        string name = html.DocumentNode.SelectSingleNode("//comment()[contains(., 'CARD NAME')]").ParentNode.InnerText.Trim();


                        if (name != "MythicSpoiler")
                        {
                            card.Name = name;
                        }
                    }
                    catch
                    { }
                    try
                    {
                        card.ManaCost = html.DocumentNode.SelectSingleNode("//comment()[contains(., 'MANA COST')]").ParentNode.InnerText.Trim();
                    }
                    catch
                    { }
                    try
                    {
                        HtmlNodeCollection nodesType = html.DocumentNode.SelectNodes("//comment()[contains(., 'TYPE')]");
                        if (nodesType.Count > 7)
                        {
                            card.Type = html.DocumentNode.SelectNodes("//comment()[contains(., 'TYPE')]")[1].ParentNode.InnerText.Trim();
                        }

                        if (card.Type.Length > 100)
                        {
                            card.Type = null;
                        }
                    }
                    catch
                    { }
                    try
                    {
                        StringBuilder sb = new();
                        HtmlNodeCollection nodes = html.DocumentNode.SelectNodes("//comment()[contains(., 'CARD TEXT')]")[16].ParentNode.ChildNodes;
                        foreach (HtmlNode node in nodes)
                        {
                            string txt = node.InnerText;
                            txt = txt.Replace("\n\n", "\n");
                            txt = txt.Replace(@"<!--CARD TEXT-->", string.Empty);
                            _ = sb.Append(txt);
                        }
                        //this code is a mess, but it works
                        string txt2 = sb.ToString();
                        txt2 = txt2.Replace("\n\n", "\n");
                        txt2 = txt2.Replace("\n\n", "\n");
                        txt2 = txt2.Replace("\n\n", "\n");
                        txt2 = txt2.Replace("\n\n", "\n");
                        txt2 = txt2.Trim();
                        card.Text = System.Net.WebUtility.HtmlDecode(txt2);
                    }
                    catch
                    { }
                    try
                    {
                        card.Flavor = System.Net.WebUtility.HtmlDecode(html.DocumentNode.SelectSingleNode("//comment()[contains(., 'FLAVOR TEXT')]").ParentNode.InnerText.Trim());
                    }
                    catch
                    { }

                    try
                    {
                        HtmlNodeCollection nodes = html.DocumentNode.SelectSingleNode("//comment()[contains(., 'P/T')]").ParentNode.ChildNodes;

                        foreach (HtmlNode node in nodes)
                        {
                            string powerToughness = node.InnerText.Trim();
                            powerToughness = powerToughness.Replace("\n", string.Empty);
                            if (powerToughness.Contains('/'))
                            {
                                string[] arrPt = powerToughness.Split('/');
                                if (arrPt.Length == 2)
                                {
                                    card.Power = arrPt[0];
                                    card.Toughness = arrPt[1];
                                    break;
                                }
                            }
                            if (powerToughness.Contains('[') &&
                                powerToughness.Contains(']')) //it is a Planeswalker and it is loyalty
                            {
                                powerToughness = powerToughness.Replace("[", string.Empty).Replace("]", string.Empty);
                                if (int.TryParse(powerToughness, out int loyalty))
                                {
                                    card.Loyalty = loyalty;
                                    break;
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
            catch
            { }
        }


        #endregion
    }
}