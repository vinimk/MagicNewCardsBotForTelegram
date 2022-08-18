using CodeHollow.FeedReader;
using HtmlAgilityPack;
using MagicNewCardsBot.Helpers;
using MagicNewCardsBot.StorageClasses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MagicNewCardsBot.TaskersAndControllers
{
    public class MagicSpoilerTasker : Tasker
    {
        #region Definitions
        protected new string _websiteUrl = "http://www.magicspoiler.com/";

        protected string _page = "feed";
        #endregion

        protected string ValidateAndFixUrl(string url)
        {
            if (!url.Contains(_websiteUrl))
            {
                url = _websiteUrl + url;
            }
            return url;
        }

        protected bool IsSameCard(string url1, string url2)
        {
            var endingUrl1 = url1[(url1.LastIndexOf('/') + 1)..];
            string endingUrl2 = url2[(url2.LastIndexOf('/') + 1)..];
            return endingUrl1 == endingUrl2;
        }

        #region Overrided Methods

        async override protected IAsyncEnumerable<Card> GetAvaliableCardsInWebSiteAsync()
        {
            //loads the website


            var feed = await FeedReader.ReadAsync(_websiteUrl + _page);
            //all the cards are a a href so we get all of that

            foreach (var item in feed.Items)
            {
                yield return new Card()
                {
                    FullUrlWebSite = item.Link
                };

            }


        }

        async override protected Task GetAdditionalInfoAsync(Card card)
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
                        card.ImageUrl = html.DocumentNode.SelectSingleNode("//div[@class='scard']").LastChild.FirstChild.Attributes["src"].Value.Trim();
                    }
                    catch { }
                    try
                    {
                        string alternativeSideImageUrl = html.DocumentNode.SelectSingleNode("//div[@class='card-content']").SelectSingleNode(".//noscript").SelectSingleNode(".//img").Attributes["src"].Value.Trim();
                        alternativeSideImageUrl = ValidateAndFixUrl(alternativeSideImageUrl);
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
                        string name = html.DocumentNode.SelectSingleNode("//h1[@class='card-title']").InnerText.Trim();

                        card.Name = System.Net.WebUtility.HtmlDecode(name);
                    }
                    catch
                    { }
                    try
                    {
                        var nodes = html.DocumentNode.SelectNodes("//li[@class='card-type']");
                        foreach (var node in nodes)
                        {
                            var nodeText = node.InnerText.Trim();
                            if (nodeText.Contains("Type:"))
                            {
                                var type = nodeText.Replace("Type:", string.Empty).Trim();
                                card.Type = type;
                            }
                        }
                    }
                    catch
                    { }
                    try
                    {
                        StringBuilder sb = new();
                        var nodes = html.DocumentNode.SelectSingleNode("//div[@class='card-content']").ChildNodes;
                        foreach (var node in nodes)
                        {
                            if (node.FirstChild != null &&
                                node.FirstChild.Name == "a" &&
                                node.FirstChild.InnerText.Contains("SRC:"))
                            {
                                card.CreditsUrl = node.FirstChild.Attributes["href"].Value.Trim();
                                card.Credits = node.FirstChild.InnerText.Replace("SRC:", string.Empty).Trim();
                                continue;
                            }
                            string txt = node.InnerText;
                            txt = txt.Replace("\n\n", "\n");
                            sb.Append(txt);
                        }
                        //this code is a mess, but it works
                        var txt2 = sb.ToString();
                        txt2 = txt2.Replace("\n\n", "\n");
                        txt2 = txt2.Replace("\n\n", "\n");
                        txt2 = txt2.Replace("\n\n", "\n");
                        txt2 = txt2.Replace("\n\n", "\n");
                        txt2 = txt2.Trim();
                        if (!string.IsNullOrWhiteSpace(txt2))
                        {
                            card.Text = System.Net.WebUtility.HtmlDecode(txt2);
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