using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MagicNewCardsBot
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
                    return String.Empty;
            }
            set
            {
                name = value;
            }
        }

        [JsonProperty("type_line")]
        public String Type
        {
            get;
            set;
        }

        [JsonProperty("oracle_text")]
        public String Text
        {
            get;
            set;
        }

        [JsonProperty("mana_cost")]
        public String ManaCost
        {
            get;
            set;
        }

        [JsonProperty("power")]
        public String Power
        {
            get;
            set;
        }

        [JsonProperty("toughness")]
        public String Toughness
        {
            get;
            set;
        }

        [JsonProperty("flavor_text")]
        public String Flavor
        {
            get;
            set;
        }
        public List<Card> ExtraSides
        {
            get;
            set;
        }
        public Boolean IsCardSent
        {
            get;
            set;
        }
        public String ImageUrl
        {
            get;
            set;
        }
        public String FullUrlWebSite
        {
            get;
            set;
        }
        public String FullInfo
        {
            get;
            set;
        }

        public String Set
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
            if (String.IsNullOrWhiteSpace(Credits) && String.IsNullOrWhiteSpace(CreditsUrl))
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
        }

        public String GetFullText()
        {
            String lineBreak = " ";
            StringBuilder sb = new();

            if (!String.IsNullOrEmpty(Name))
            {
                sb.Append(Name);
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(ManaCost))
            {
                sb.AppendFormat("|{0}|", ManaCost);
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(Type))
            {
                sb.AppendFormat("{0}.", Type);
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(Text))
            {
                sb.Append(Text);
                if (!sb.ToString().EndsWith("."))
                {
                    sb.Append('.');
                }
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(Power) || (!String.IsNullOrEmpty(Toughness)))
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

        public String GetTelegramTextFormatted()
        {
            String lineBreak = Environment.NewLine;
            StringBuilder sb = new();

            if (!String.IsNullOrEmpty(Name))
            {
                sb.AppendFormat("<b>{0}</b>", WebUtility.HtmlEncode(Name));
            }

            if (!String.IsNullOrEmpty(ManaCost))
            {
                sb.AppendFormat(" - {0}", WebUtility.HtmlEncode(ManaCost));
                sb.Append(lineBreak);
            }
            else
            {
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(Type))
            {
                sb.AppendFormat("<i>{0}</i>", WebUtility.HtmlEncode(Type));
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(Text))
            {
                sb.Append(WebUtility.HtmlEncode(Text));
                if (!sb.ToString().EndsWith("."))
                {
                    sb.Append('.');
                }
                sb.Append(lineBreak);
            }



            if (!String.IsNullOrEmpty(Power) || (!String.IsNullOrEmpty(Toughness)))
            {
                sb.Append(String.Format("<b>P/T: {0}/{1}</b>", Power, Toughness));
                sb.Append(lineBreak);
            }

            if (Loyalty.HasValue)
            {
                sb.Append(String.Format("<b>Loyalty:</b> {0}", Loyalty.Value));
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(Flavor))
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

        public String GetTwitterText()
        {
            string text = GetFullText() + " #MTG #MagicTheGathering";
            if (text.Length < 240)
            {
                return text.Replace(URL_REPLACE_TEXT, CreditsUrl);
            }
            else
            {
                String lineBreak = Environment.NewLine;
                StringBuilder sb = new();
                if (!String.IsNullOrEmpty(Name))
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

        public override string ToString()
        {
            if (!String.IsNullOrWhiteSpace(Name))
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
            if (this.ExtraSides == null)
                this.ExtraSides = new List<Card>();

            this.ExtraSides.Add(card);
        }

        #endregion
    }
}