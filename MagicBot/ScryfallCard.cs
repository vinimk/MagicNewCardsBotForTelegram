using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace MagicBot
{
    public class ScryfallCard
    {
        public String id { get; set; }
        public String scryfall_uri { get; set; }

        public String name;

        public String layout { get; set; }
        public String type_line { get; set; }
        public String oracle_text { get; set; }
        public String mana_cost { get; set; }
        public String power { get; set; }
        public String toughness { get; set; }
        public String set_name { get; set; }
        public JToken image_uris { get; set; }

        public String flavor_text { get; set; }

        public String artist { get; set; }

        public String set { get; set; }

        public JToken card_faces { get; set; }

        [NotMapped]
        public List<ScryfallCard> ExtraSides{ get; set; }

        [NotMapped]
        public Boolean IsCardSent { get; set; }

        [NotMapped]
        public string image_url { get; set; }

        #region Methods

        public override String ToString()
        {
            return name;
        }

        public String GetTelegramText()
        {
            String lineBreak = " ";
            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(name))
            {
                sb.Append(name);
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(mana_cost))
            {
                sb.AppendFormat("|{0}|", mana_cost);
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(type_line))
            {
                sb.AppendFormat("{0}.", type_line);
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(oracle_text))
            {
                sb.Append(oracle_text);
                if (!sb.ToString().EndsWith("."))
                {
                    sb.Append(".");
                }
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(power) || (!String.IsNullOrEmpty(toughness)))
            {
                sb.AppendFormat(" ({0}/{1})", power, toughness);
                sb.Append(lineBreak);
            }

            sb.Append(scryfall_uri);

            return sb.ToString().Replace("\n", " ").Replace("  ", " "); //this is a linebreak for telegram API
        }

        public String GetTelegramTextFormatted()
        {
            String lineBreak = Environment.NewLine;
            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(name))
            {
                sb.AppendFormat("<b>{0}</b>", WebUtility.HtmlEncode(name));
            }
            

            if (!String.IsNullOrEmpty(mana_cost))
            {
                sb.AppendFormat(" - {0}", WebUtility.HtmlEncode(mana_cost));
                sb.Append(lineBreak);
            }
            else
            {
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(type_line))
            {
                sb.AppendFormat("<i>{0}</i>", WebUtility.HtmlEncode(type_line));
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(oracle_text))
            {
                sb.Append(WebUtility.HtmlEncode(oracle_text));
                if (!sb.ToString().EndsWith("."))
                {
                    sb.Append(".");
                }
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(flavor_text))
            {
                sb.AppendFormat("<i>{0}</i>", WebUtility.HtmlEncode(flavor_text));
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(power) || !String.IsNullOrEmpty(toughness))
            {
                sb.Append(String.Format("<b>P/T: {0}/{1}</b>", power, toughness));
                sb.Append(lineBreak);
            }

            sb.Append(scryfall_uri);


            return sb.ToString();
        }

        public String GetTwitterText()
        {
            String lineBreak = Environment.NewLine;
            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(name))
            {
                sb.Append(name);
            }


            if (!String.IsNullOrEmpty(type_line))
            {
                sb.Append(" - ");
                sb.Append(type_line);
            }

            sb.Append(" #MTG #MagicTheGathering");

            sb.Append(lineBreak);
            sb.Append(scryfall_uri);

            return sb.ToString();
        }

        #endregion
    }
}
