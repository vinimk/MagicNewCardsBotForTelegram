namespace MagicNewCardsBot
{
    //public class MythicApiTasker : Tasker
    //{
    //    #region Definitions
    //    private readonly string _websiteUrl = "https://mythicspoiler.com/";
    //    private readonly string _pathNewCards = "newspoilers.html";
    //    #endregion

    //    #region Overrided Methods

    //    async override protected Task<List<Card>> GetAvaliableCardsInWebSite()
    //    {
    //        List<Card> lstCards = new List<Card>();
    //        try
    //        {
    //            //loads the website

    //            HtmlDocument doc = new HtmlDocument();
    //            //crawl the webpage to get this information
    //            using (Stream stream = await Program.GetStreamFromUrlAsync(String.Format("{0}{1}", _websiteUrl, _pathNewCards)))
    //            {
    //                doc.Load(stream);
    //            }

    //            //all the cards are a a href so we get all of that
    //            HtmlNodeCollection nodesCards = doc.DocumentNode.SelectNodes("//img");
    //            foreach (HtmlNode node in nodesCards)
    //            {
    //                //also the cards have a special class called 'card', so we use it to get the right ones
    //                if (node.Attributes.Contains("src") &&
    //                    node.Attributes["src"].Value.ToString().Contains("cards") &&
    //                    node.Attributes["src"].Value.ToString().EndsWith(".jpg"))
    //                {
    //                    Card card = new Card
    //                    {
    //                        FullUrlWebSite = _websiteUrl + node.ParentNode.Attributes["href"].Value.ToString(),
    //                        ImageUrl = _websiteUrl + node.Attributes["src"].Value.ToString()
    //                    };
    //                    lstCards.Add(card);
    //                }

    //                //only get the lastest 50
    //                if (lstCards.Count == Database.MAX_CARDS)
    //                    break;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Database.InsertLog("Error crawling the main page", String.Empty, ex.ToString()).Wait();
    //            Program.WriteLine("Error crawling the main page");
    //            Program.WriteLine(ex.Message);
    //            Program.WriteLine(ex.StackTrace);
    //        }

    //        return lstCards;
    //    }


    //    async override protected Task<Card> GetAdditionalInfo(Card spoil)
    //    {
    //        //we do all of this in empty try catches because it is not mandatory information
    //        try
    //        {
    //            HtmlDocument html = new HtmlDocument();
    //            //crawl the webpage to get this information
    //            using (Stream stream = await Program.GetStreamFromUrlAsync(spoil.FullUrlWebSite))
    //            {
    //                html.Load(stream);
    //            }

    //            try
    //            {
    //                var node2 = html.DocumentNode.SelectSingleNode("//comment()[contains(., 'THE individual card')]");
    //                foreach (HtmlNode possibleImage in node2.ParentNode.ChildNodes)
    //                {
    //                    if (possibleImage.Attributes["src"] != null)
    //                    {
    //                        Uri url = new Uri(spoil.FullUrlWebSite);
    //                        string possible = url.AbsoluteUri.Remove(url.AbsoluteUri.Length - url.Segments.Last().Length) + possibleImage.Attributes["src"].Value.ToString();
    //                        if (possible != spoil.ImageUrl)
    //                        {
    //                            Card extraSide = new Card
    //                            {
    //                                ImageUrl = possible
    //                            };
    //                            spoil.ExtraSides.Add(extraSide);
    //                        }
    //                    }
    //                }
    //            }
    //            catch
    //            { }

    //            try
    //            {
    //                string name = html.DocumentNode.SelectSingleNode(".//title").InnerText.Trim();
    //                int indexSeparator = name.IndexOf('|');
    //                if (name.Length >= indexSeparator)
    //                    name = name.Substring(0, name.IndexOf('|')).Trim();

    //                if (name != "MythicSpoiler")
    //                {
    //                    spoil.Name = name;
    //                }
    //            }
    //            catch
    //            { }
    //            try
    //            {
    //                spoil.ManaCost = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[2]/td[1]").LastChild.InnerText.Trim();
    //            }
    //            catch
    //            { }
    //            try
    //            {
    //                spoil.Type = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[3]/td[1]").LastChild.InnerText.Trim();
    //            }
    //            catch
    //            { }
    //            try
    //            {
    //                StringBuilder sb = new StringBuilder();
    //                var nodes = html.DocumentNode.SelectNodes("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[4]/td[1]");
    //                foreach (var node in nodes)
    //                {
    //                    String txt = node.InnerText;
    //                    txt = txt.Replace("\n\n", "\n");
    //                    txt = txt.Replace(@"<!--CARD TEXT-->", String.Empty);
    //                    sb.Append(txt.Trim());
    //                }
    //                spoil.Text = System.Net.WebUtility.HtmlDecode(sb.ToString());
    //            }
    //            catch
    //            { }
    //            try
    //            {
    //                spoil.Flavor = System.Net.WebUtility.HtmlDecode(html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[5]/td[1]/i[1]").LastChild.InnerText.Trim());
    //            }
    //            catch
    //            { }

    //            try
    //            {
    //                var nodes = html.DocumentNode.SelectNodes("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[7]/td[2]/font[1]");

    //                foreach (var node in nodes)
    //                {
    //                    var powerToughness = node.ChildNodes[2].InnerText.Trim();
    //                    powerToughness = powerToughness.Replace("\n", String.Empty);
    //                    if (powerToughness.Contains("/"))
    //                    {
    //                        String[] arrPt = powerToughness.Split('/');
    //                        if (arrPt.Length == 2)
    //                        {
    //                            spoil.Power = arrPt[0];
    //                            spoil.Toughness = arrPt[1];
    //                            break;
    //                        }
    //                    }
    //                }

    //            }
    //            catch
    //            { }
    //        }
    //        catch
    //        { }
    //        return spoil;
    //    }

    //    #endregion
    //}
}