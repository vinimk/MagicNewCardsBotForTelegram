using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.IO;
using ImageSharp;

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

        public String TelegramText()
        {
            String lineBreak = "%0A";
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
            if (!String.IsNullOrEmpty(Type))
            {
                sb.Append(Type);
                if (!String.IsNullOrEmpty(ManaCost))
                {
                    sb.Append("-");
                    sb.Append(ManaCost);
                }
            }
            else if (!String.IsNullOrEmpty(ManaCost))
            {
                sb.Append(ManaCost);
            }
            if (!String.IsNullOrEmpty(Text))
            {
                sb.Append(Text);
                sb.Append(lineBreak);
            }
            if (!String.IsNullOrEmpty(Flavor))
            {
                sb.Append(Flavor);
                sb.Append(lineBreak);
            }
            if (Power >= 0 || Toughness >= 0)
            {
                sb.Append(String.Format("P/T{0}-{1}", Power, Toughness));
                sb.Append(lineBreak);
            }
            sb.Append(FullUrlWebSite);
            sb.Append(lineBreak);


            return sb.ToString(); //this is a linebreak for telegram API
        }

        public String TwitterText()
        {
            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrEmpty(Name))
            {
                sb.AppendLine(Name);
            }
            else
            {
                sb.AppendLine(CardUrl);
            }

            if (!String.IsNullOrEmpty(Type))
            {
                sb.Append(Type);
                if (!String.IsNullOrEmpty(ManaCost))
                {
                    sb.Append("-");
                    sb.Append(ManaCost);
                }
            }
            else if (!String.IsNullOrEmpty(ManaCost))
            {
                sb.Append(ManaCost);
            }

            sb.AppendLine(FullUrlWebSite);

            return sb.ToString();
        }
    }
}
