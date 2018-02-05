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

namespace MagicBot
{
    public class MythicApiTasker
    {
        #region Definitions
        private String _websiteUrl;
        private String _pathNewCards;
        #endregion

        #region Constructors
        public MythicApiTasker(String websiteUrl, String pathNewCards)
        {
            _websiteUrl = websiteUrl;
            _pathNewCards = pathNewCards;
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

        #region Public Methods
        public void GetNewCards()
        {
            CheckNewCards();
        }
        #endregion

        #region Private Methods
        private void CheckNewCards()
        {
            //get the aditional infos from the website
            List<Card> lstCards = GetAvaliableCardsInWebSite();
            foreach (Card card in lstCards)
            {
                CheckCard(card);
            }
        }

        private void CheckCard(Card card)
        {
            //check if the spoil is in the database
            //if is not in the database AND has NOT been sent
            if (!Database.IsCardInDatabase(card, true))
            {
                card = GetAdditionalInfo(card);
                //adds in the database
                Database.InsertScryfallCard(card);

                //fires the event to do stuffs with the new object
                OnNewCard(card);
            }
        }

        private Card GetAdditionalInfo(Card spoil)
        {
            //we do all of this in empty try catches because it is not mandatory information
            try
            {
                //crawl the webpage to get this information
                HtmlWeb htmlWeb = new HtmlWeb();
                HtmlDocument html = htmlWeb.Load(spoil.FullUrlWebSite);

                try
                {
                    var node2 = html.DocumentNode.SelectSingleNode("//comment()[contains(., 'THE individual card')]");
                    foreach (HtmlNode possibleImage in node2.ParentNode.ChildNodes)
                    {
                        if (possibleImage.Attributes["src"] != null)
                        {
                            Uri url = new Uri(spoil.FullUrlWebSite);
                            string possible = url.AbsoluteUri.Remove(url.AbsoluteUri.Length - url.Segments.Last().Length)+ possibleImage.Attributes["src"].Value.ToString();
                            if (possible != spoil.ImageUrl)
                            {
                                Card extraSide = new Card();
                                extraSide.ImageUrl = possible;
                                spoil.ExtraSides.Add(extraSide);
                            }
                        }
                    }
                }
                catch
                { }

                try
                {
                    spoil.Name = html.DocumentNode.SelectSingleNode("html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[1]/td[1]/font[1]").LastChild.InnerText.Trim();
                }
                catch
                { }
                try
                {
                    spoil.ManaCost = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[2]/td[1]").LastChild.InnerText.Trim();
                }
                catch
                { }
                try
                {
                    spoil.Type = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[3]/td[1]").LastChild.InnerText.Trim();
                }
                catch
                { }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    var nodes = html.DocumentNode.SelectNodes("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[4]/td[1]");
                    foreach (var node in nodes)
                    {
                        String txt = node.InnerText;
                        txt = txt.Replace("\n\n", "\n");
                        txt = txt.Replace(@"<!--CARD TEXT-->", String.Empty);
                        sb.Append(txt.Trim());
                    }
                    spoil.Text = System.Net.WebUtility.HtmlDecode(sb.ToString());
                }
                catch
                { }
                try
                {
                    spoil.Flavor = System.Net.WebUtility.HtmlDecode(html.DocumentNode.SelectSingleNode("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[5]/td[1]/i[1]").LastChild.InnerText.Trim());
                }
                catch
                { }

                try
                {
                    var nodes = html.DocumentNode.SelectNodes("/html[1]/body[1]/center[1]/table[5]/tr[1]/td[2]/font[1]/center[1]/table[1]/tr[7]/td[2]/font[1]");

                    foreach (var node in nodes)
                    {
                        var powerToughness = node.ChildNodes[2].InnerText.Trim();
                        powerToughness = powerToughness.Replace("\n", String.Empty);
                        if (powerToughness.Contains("/"))
                        {
                            String[] arrPt = powerToughness.Split('/');
                            if (arrPt.Length == 2)
                            {
                                spoil.Power = arrPt[0];
                                spoil.Toughness = arrPt[1];
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
            return spoil;
        }

        private List<Card> GetAvaliableCardsInWebSite()
        {
            List<Card> lstCards = new List<Card>();
            try
            {
                //loads the website
                HtmlWeb htmlWeb = new HtmlWeb();
                HtmlDocument doc = htmlWeb.Load(String.Format("{0}{1}", _websiteUrl, _pathNewCards));

                //all the cards are a a href so we get all of that
                HtmlNodeCollection nodesCards = doc.DocumentNode.SelectNodes("//img");
                foreach (HtmlNode node in nodesCards)
                {
                    //also the cards have a special class called 'card', so we use it to get the right ones
                    if (node.Attributes.Contains("src")&&
                        node.Attributes["src"].Value.ToString().Contains("cards")&&
                        node.Attributes["src"].Value.ToString().EndsWith(".jpg"))
                    {
                        Card card = new Card();
                        card.FullUrlWebSite = _websiteUrl + node.ParentNode.Attributes["href"].Value.ToString();
                        card.ImageUrl = _websiteUrl + node.Attributes["src"].Value.ToString();
                        lstCards.Add(card);
                    }

                    //only get the lastest 50
                    if (lstCards.Count == 50)
                        break;
                }
            }
            catch (Exception ex)
            {
                Database.InsertLog("Error crawling the main page", String.Empty, ex.ToString());
                Program.WriteLine("Error crawling the main page");
                Program.WriteLine(ex.Message);
                Program.WriteLine(ex.StackTrace);
            }

            return lstCards;
        }

        #endregion
    }
}