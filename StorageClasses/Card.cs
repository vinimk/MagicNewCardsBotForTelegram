using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MagicNewCardsBot
{
    public class Card
    {
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

        #region Methods

        public Card()
        {
            ExtraSides = new List<Card>();
        }

        public String GetFullText()
        {
            String lineBreak = " ";
            StringBuilder sb = new StringBuilder();

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
                    sb.Append(".");
                }
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(Power) || (!String.IsNullOrEmpty(Toughness)))
            {
                sb.AppendFormat(" ({0}/{1})", Power, Toughness);
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
            return text;
        }

        public String GetTelegramTextFormatted()
        {
            String lineBreak = Environment.NewLine;
            StringBuilder sb = new StringBuilder();

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
                    sb.Append(".");
                }
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(Flavor))
            {
                sb.AppendFormat("<i>{0}</i>", WebUtility.HtmlEncode(Flavor));
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

            sb.Append(FullUrlWebSite);

            return sb.ToString();
        }

        public String GetTwitterText()
        {
            string text = GetFullText() + " #MTG #MagicTheGathering";
            if (text.Length < 240)
            {
                return text;
            }
            else
            {
                String lineBreak = Environment.NewLine;
                StringBuilder sb = new StringBuilder();
                if (!String.IsNullOrEmpty(Name))
                {
                    sb.Append(Name);
                }

                if (!String.IsNullOrEmpty(Type))
                {
                    sb.Append(" - ");
                    sb.Append(Type);
                }

                sb.Append(" #MTG #MagicTheGathering");

                sb.Append(lineBreak);
                sb.Append(FullUrlWebSite);

                return sb.ToString();
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