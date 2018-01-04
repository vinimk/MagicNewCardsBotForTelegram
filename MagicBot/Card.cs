using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MagicBot
{
    public class Card
    {
        [JsonProperty("name")]
        public String Name { get; set; }
        [JsonProperty("type_line")]
        public String Type { get; set; }
        [JsonProperty("oracle_text")]
        public String Text { get; set; }
        [JsonProperty("mana_cost")]
        public String ManaCost { get; set; }
        [JsonProperty("power")]
        public String Power { get; set; }
        [JsonProperty("toughness")]
        public String Toughness { get; set; }
        [JsonProperty("flavor_text")]
        public String Flavor { get; set; }
        public List<Card> ExtraSides { get; set; }
        public Boolean IsCardSent { get; set; }
        public String ImageUrl { get; set; }
        public String FullUrlWebSite { get; set; }

        #region Methods

        public Card()
        {
            ExtraSides = new List<Card>();
        }

        public String GetTelegramText()
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

            return sb.ToString().Replace("\n", " ").Replace("  ", " "); //this is a linebreak for telegram API
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

            sb.Append(FullUrlWebSite);


            return sb.ToString();
        }

        public String GetTwitterText()
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

        #endregion
    }
}