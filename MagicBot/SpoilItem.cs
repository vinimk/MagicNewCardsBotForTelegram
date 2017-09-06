using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.IO;
using ImageSharp;
using System.Net;

namespace MagicBot
{
    public class SpoilItem
    {
        public Int64 SpoilItemId { get; set; }
        public String Folder { get; set; }
        public DateTime Date { get; set; }
        public String CardUrl { get; set; }
        public String FullUrlWebSite { get; set; }
        public String Name { get; set; }
        public String ManaCost { get; set; }
        public String Type { get; set; }
        public String Text { get; set; }
        public String Flavor { get; set; }
        public String Illustrator { get; set; }
        private Int32 power = -1;
        public Int32 Power
        {
            get
            {
                return power;
            }
            set
            {
                if (value >= 0)
                {
                    power = value;
                }
            }
        }

        private Int32 toughness = -1;
        public Int32 Toughness
        {
            get
            {
                return toughness;
            }
            set
            {
                if (value >= 0)
                {
                    toughness = value;
                }
            }
        }
        public String ImageUrlWebSite { get; set; }
        public String AdditionalImageUrlWebSite { get; set; }
        [NotMapped]
        public Image<Rgba32> Image { get; set; }
        [NotMapped]
        public Image<Rgba32> AdditionalImage { get; set; }

        public override String ToString()
        {
            //if it doesn't have name, returns the cardUrl
            if (String.IsNullOrEmpty(Name))
            {
                return CardUrl;
            }
            else
            {
                return Name;
            }
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
            else
            {
                sb.Append(CardUrl);
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(ManaCost))
            {
                sb.Append("- ");
                sb.Append(ManaCost);
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(Type))
            {
                sb.Append(Type);
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(Text))
            {
                sb.Append(Text);
                sb.Append(lineBreak);
            }

            sb.Append(FullUrlWebSite);

            return sb.ToString(); //this is a linebreak for telegram API
        }

        public String GetTelegramTextFormatted()
        {
            String lineBreak = Environment.NewLine;
            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(Name))
            {
                sb.AppendFormat("<b>{0}</b>",WebUtility.HtmlEncode(Name));
            }
            else
            {
                sb.AppendFormat("<b>{0}</b>",WebUtility.HtmlEncode(CardUrl));
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
                sb.Append(lineBreak);
            }

            if (!String.IsNullOrEmpty(Flavor))
            {
                sb.AppendFormat("<i>{0}</i>", WebUtility.HtmlEncode(Flavor));
                sb.Append(lineBreak);
            }

            if (Power >= 0 || Toughness >= 0)
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
            else
            {
                sb.Append(CardUrl);
            }

            if (!String.IsNullOrEmpty(Type))
            {
                sb.Append(" - ");
                sb.Append(Type);
            }

            sb.Append(lineBreak);
            sb.Append(FullUrlWebSite);

            return sb.ToString();
        }
    }
}
