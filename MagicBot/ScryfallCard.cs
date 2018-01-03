using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.IO;
using SixLabors.ImageSharp;
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
        public ScryfallCard SecondSide{ get; set;}

        [NotMapped]
        public Boolean IsCardSent { get; set; }
        [NotMapped]
        public Image<Rgba32> Image { get; set; }
        [NotMapped]
        #region Methods

        // public override String ToString()
        // {
        //     //if it doesn't have name, returns the cardUrl
        //     if (String.IsNullOrEmpty(Name))
        //     {
        //         return CardUrl;
        //     }
        //     else
        //     {
        //         return Name;
        //     }
        // }

        // public String GetTelegramText()
        // {
        //     String lineBreak = " ";
        //     StringBuilder sb = new StringBuilder();

        //     if (!String.IsNullOrEmpty(Name))
        //     {
        //         sb.Append(Name);
        //         sb.Append(lineBreak);
        //     }
        //     else
        //     {
        //         sb.Append(CardUrl.Replace(".jpg",String.Empty));
        //         sb.Append(lineBreak);
        //     }

        //     if (!String.IsNullOrEmpty(ManaCost))
        //     {
        //         sb.AppendFormat("|{0}|", ManaCost);
        //         sb.Append(lineBreak);
        //     }

        //     if (!String.IsNullOrEmpty(Type))
        //     {
        //         sb.AppendFormat("{0}.", Type);
        //         sb.Append(lineBreak);
        //     }

        //     if (!String.IsNullOrEmpty(Text))
        //     {
        //         sb.Append(Text);
        //         if (!sb.ToString().EndsWith("."))
        //         {
        //             sb.Append(".");
        //         }
        //         sb.Append(lineBreak);
        //     }

        //     if (Power >= 0 || Toughness >= 0)
        //     {
        //         sb.AppendFormat(" ({0}/{1})", Power, Toughness);
        //         sb.Append(lineBreak);
        //     }

        //     sb.Append(FullUrlWebSite);

        //     return sb.ToString().Replace("\n", " ").Replace("  ", " "); //this is a linebreak for telegram API
        // }

        // public String GetTelegramTextFormatted()
        // {
        //     String lineBreak = Environment.NewLine;
        //     StringBuilder sb = new StringBuilder();

        //     if (!String.IsNullOrEmpty(Name))
        //     {
        //         sb.AppendFormat("<b>{0}</b>", WebUtility.HtmlEncode(Name));
        //     }
        //     else
        //     {
        //         sb.AppendFormat("<b>{0}</b>", WebUtility.HtmlEncode(CardUrl.Replace(".jpg",String.Empty)));
        //     }

        //     if (!String.IsNullOrEmpty(ManaCost))
        //     {
        //         sb.AppendFormat(" - {0}", WebUtility.HtmlEncode(ManaCost));
        //         sb.Append(lineBreak);
        //     }
        //     else
        //     {
        //         sb.Append(lineBreak);
        //     }

        //     if (!String.IsNullOrEmpty(Type))
        //     {
        //         sb.AppendFormat("<i>{0}</i>", WebUtility.HtmlEncode(Type));
        //         sb.Append(lineBreak);
        //     }

        //     if (!String.IsNullOrEmpty(Text))
        //     {
        //         sb.Append(WebUtility.HtmlEncode(Text));
        //         if (!sb.ToString().EndsWith("."))
        //         {
        //             sb.Append(".");
        //         }
        //         sb.Append(lineBreak);
        //     }

        //     if (!String.IsNullOrEmpty(Flavor))
        //     {
        //         sb.AppendFormat("<i>{0}</i>", WebUtility.HtmlEncode(Flavor));
        //         sb.Append(lineBreak);
        //     }

        //     if (Power >= 0 || Toughness >= 0)
        //     {
        //         sb.Append(String.Format("<b>P/T: {0}/{1}</b>", Power, Toughness));
        //         sb.Append(lineBreak);
        //     }

        //     sb.Append(FullUrlWebSite);


        //     return sb.ToString();
        // }

        // public String GetTwitterText()
        // {
        //     String lineBreak = Environment.NewLine;
        //     StringBuilder sb = new StringBuilder();
        //     if (!String.IsNullOrEmpty(Name))
        //     {
        //         sb.Append(Name);
        //     }
        //     else
        //     {
        //         sb.Append(CardUrl.Replace(".jpg",String.Empty));
        //     }

        //     if (!String.IsNullOrEmpty(Type))
        //     {
        //         sb.Append(" - ");
        //         sb.Append(Type);
        //     }

        //     sb.Append(" #MTG");

        //     sb.Append(lineBreak);
        //     sb.Append(FullUrlWebSite);

        //     return sb.ToString();
        // }


        // public Boolean HasAnyExtraInfo()
        // {
        //     if (!String.IsNullOrEmpty(ManaCost))
        //     {
        //         return true;
        //     }
        //     if (!String.IsNullOrEmpty(Type))
        //     {
        //         return true;
        //     }
        //     if (!String.IsNullOrEmpty(Text))
        //     {
        //         return true;
        //     }
        //     if (!String.IsNullOrEmpty(Flavor))
        //     {
        //         return true;
        //     }
        //     if (!String.IsNullOrEmpty(Illustrator))
        //     {
        //         return true;
        //     }
        //     if (Power > 0)
        //     {
        //         return true;
        //     }
        //     if (Toughness > 0)
        //     {
        //         return true;
        //     }
        //     if (AdditionalImage != null)
        //     {
        //         return true;
        //     }

        //     return false;

        // }
        #endregion
    }
}
