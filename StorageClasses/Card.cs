using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MagicNewCardsBot.StorageClasses
{

    public class Card
    {
        private static readonly string URL_REPLACE_TEXT = "twitterUrl23Characters";
        private string name;
        [JsonProperty("name")]
        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(name))
                    return name;
                else
                    return string.Empty;
            }
            set
            {
                name = value;
            }
        }

        [JsonProperty("type_line")]
        public string Type
        {
            get;
            set;
        }

        [JsonProperty("oracle_text")]
        public string Text
        {
            get;
            set;
        }

        [JsonProperty("mana_cost")]
        public string ManaCost
        {
            get;
            set;
        }

        [JsonProperty("power")]
        public string Power
        {
            get;
            set;
        }

        [JsonProperty("toughness")]
        public string Toughness
        {
            get;
            set;
        }

        [JsonProperty("flavor_text")]
        public string Flavor
        {
            get;
            set;
        }

        public Rarity? Rarity
        {
            get;
            set;
        }

        public List<Card> ExtraSides
        {
            get;
            set;
        }
        public SendTo SendTo
        {
            get;
            set;
        }
        public string ImageUrl
        {
            get;
            set;
        }
        public string FullUrlWebSite
        {
            get;
            set;
        }
        public string FullInfo
        {
            get;
            set;
        }
        public string Set
        {
            get;
            set;
        }
        public int? Loyalty
        {
            get;
            set;
        }
        public string CreditsUrl
        {
            get;
            set;
        }
        public string Credits
        {
            get;
            set;
        }

        //we blacklist some URLs for credits that are from wizards or something like that
        public bool UseCredits()
        {
            if (string.IsNullOrWhiteSpace(Credits) && string.IsNullOrWhiteSpace(CreditsUrl))
                return false;
            if (CreditsUrl.Contains("card-image-gallery"))
                return false;
            if (CreditsUrl.Contains("www.twitch.tv/magic"))
                return false;
            return true;
        }

        #region Methods

        public Card()
        {
            ExtraSides = new List<Card>();
            SendTo = SendTo.Both;
        }

        public string GetFullText()
        {
            string lineBreak = " ";
            StringBuilder sb = new();

            if (!string.IsNullOrEmpty(Name))
            {
                sb.Append(Name);
                sb.Append(lineBreak);
            }

            if (!string.IsNullOrEmpty(ManaCost))
            {
                sb.AppendFormat("|{0}|", ManaCost);
                sb.Append(lineBreak);
            }

            if (!string.IsNullOrEmpty(Type))
            {
                sb.AppendFormat("{0}.", Type);
                sb.Append(lineBreak);
            }

            if (!string.IsNullOrEmpty(Text))
            {
                sb.Append(Text);
                if (!sb.ToString().EndsWith("."))
                {
                    sb.Append('.');
                }
                sb.Append(lineBreak);
            }

            if (!string.IsNullOrEmpty(Power) || !string.IsNullOrEmpty(Toughness))
            {
                sb.AppendFormat(" ({0}/{1})", Power, Toughness);
                sb.Append(lineBreak);
            }

            if (UseCredits())
            {
                sb.Append(URL_REPLACE_TEXT);
                sb.Append(lineBreak);
            }

            sb.Append(FullUrlWebSite);

            return sb.ToString();
        }

        public string GetTwitterAltText()
        {
            var text = GetFullText();
            if (text.Length > 1000)
                text = text.Substring(0, 999);
            return text.Replace(URL_REPLACE_TEXT, CreditsUrl);
        }

        public string GetTelegramTextFormatted()
        {
            string lineBreak = Environment.NewLine;
            StringBuilder sb = new();

            if (!string.IsNullOrEmpty(Name))
            {
                sb.AppendFormat("<b>{0}</b>", WebUtility.HtmlEncode(Name));
            }

            if (!string.IsNullOrEmpty(ManaCost))
            {
                sb.AppendFormat(" - {0}", WebUtility.HtmlEncode(ManaCost));
                sb.Append(lineBreak);
            }
            else
            {
                sb.Append(lineBreak);
            }

            if (!string.IsNullOrEmpty(Type))
            {
                sb.AppendFormat("<i>{0}</i>", WebUtility.HtmlEncode(Type));
                sb.Append(lineBreak);
            }

            if (!string.IsNullOrEmpty(Text))
            {
                sb.Append(WebUtility.HtmlEncode(Text));
                if (!sb.ToString().EndsWith("."))
                {
                    sb.Append('.');
                }
                sb.Append(lineBreak);
            }



            if (!string.IsNullOrEmpty(Power) || !string.IsNullOrEmpty(Toughness))
            {
                sb.Append(string.Format("<b>P/T: {0}/{1}</b>", Power, Toughness));
                sb.Append(lineBreak);
            }

            if (Loyalty.HasValue)
            {
                sb.Append(string.Format("<b>Loyalty:</b> {0}", Loyalty.Value));
                sb.Append(lineBreak);
            }

            if (!string.IsNullOrEmpty(Flavor))
            {
                sb.AppendFormat("<i>{0}</i>", WebUtility.HtmlEncode(Flavor));
                sb.Append(lineBreak);
            }

            if (UseCredits())
            {
                sb.Append($"Revealed by: <a href='{CreditsUrl}'>{Credits}</a>");
                sb.Append(lineBreak);
            }

            sb.Append($"<a href='{FullUrlWebSite}'><i>Link</i></a>");

            return sb.ToString();
        }

        public string GetTwitterText()
        {
            string text = GetFullText() + " #MTG ";
            if (text.Length < 240)
            {
                return text.Replace(URL_REPLACE_TEXT, CreditsUrl);
            }
            else
            {
                string lineBreak = Environment.NewLine;
                StringBuilder sb = new();
                if (!string.IsNullOrEmpty(Name))
                {
                    sb.Append(Name);
                }

                sb.Append(" #MTG ");

                if (UseCredits())
                {
                    sb.Append($"Revealed by: {CreditsUrl}");
                    sb.Append(lineBreak);
                }

                sb.Append(lineBreak);
                sb.Append(FullUrlWebSite);

                var txt = sb.ToString();
                if (txt.Length > 240)
                {
                    txt = txt.Substring(0, 239);
                }
                return txt;
            }
        }
        public string GetRarityCharacter()
        {
            if (Rarity.HasValue)
            {
                return Rarity switch
                {
                    StorageClasses.Rarity.Common => "C",
                    StorageClasses.Rarity.Uncommon => "U",
                    StorageClasses.Rarity.Rare => "R",
                    StorageClasses.Rarity.Mythic => "M",
                    _ => string.Empty,
                };
            }
            return string.Empty;
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                return $"{Name}";
            }
            else
            {
                return FullUrlWebSite;
            }
        }

        public void AddExtraSide(Card card)
        {
            if (ExtraSides == null)
                ExtraSides = new List<Card>();
            if (!ExtraSides.Exists(x => x.ImageUrl.Equals(card.ImageUrl)))
            {
                ExtraSides.Add(card);
            }
        }

        #endregion
    }

    public enum Rarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Mythic = 3
    }

    public enum SendTo
    {
        Both = 0,
        OnlyRarity = 1,
        OnlyAll = 2
    }
}