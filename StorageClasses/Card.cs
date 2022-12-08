using Newtonsoft.Json;
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
            get => !string.IsNullOrEmpty(name) ? name : string.Empty;
            set => name = value;
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
            {
                return false;
            }

            return !CreditsUrl.Contains("card-image-gallery") && !CreditsUrl.Contains("www.twitch.tv/magic");
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
                _ = sb.Append(Name);
                _ = sb.Append(lineBreak);
            }

            if (!string.IsNullOrEmpty(ManaCost))
            {
                _ = sb.AppendFormat("|{0}|", ManaCost);
                _ = sb.Append(lineBreak);
            }

            if (!string.IsNullOrEmpty(Type))
            {
                _ = sb.AppendFormat("{0}.", Type);
                _ = sb.Append(lineBreak);
            }

            if (!string.IsNullOrEmpty(Text))
            {
                _ = sb.Append(Text);
                if (!sb.ToString().EndsWith("."))
                {
                    _ = sb.Append('.');
                }
                _ = sb.Append(lineBreak);
            }

            if (!string.IsNullOrEmpty(Power) || !string.IsNullOrEmpty(Toughness))
            {
                _ = sb.AppendFormat(" ({0}/{1})", Power, Toughness);
                _ = sb.Append(lineBreak);
            }

            if (UseCredits())
            {
                _ = sb.Append(URL_REPLACE_TEXT);
                _ = sb.Append(lineBreak);
            }

            _ = sb.Append(FullUrlWebSite);

            return sb.ToString();
        }

        public string GetTwitterAltText()
        {
            string text = GetFullText();
            if (text.Length > 1000)
            {
                text = text[..999];
            }

            return text.Replace(URL_REPLACE_TEXT, CreditsUrl);
        }

        public string GetTelegramTextFormatted()
        {
            string lineBreak = Environment.NewLine;
            StringBuilder sb = new();

            if (!string.IsNullOrEmpty(Name))
            {
                _ = sb.AppendFormat("<b>{0}</b>", WebUtility.HtmlEncode(Name));
            }

            if (!string.IsNullOrEmpty(ManaCost))
            {
                _ = sb.AppendFormat(" - {0}", WebUtility.HtmlEncode(ManaCost));
                _ = sb.Append(lineBreak);
            }
            else
            {
                _ = sb.Append(lineBreak);
            }

            if (!string.IsNullOrEmpty(Type))
            {
                _ = sb.AppendFormat("<i>{0}</i>", WebUtility.HtmlEncode(Type));
                _ = sb.Append(lineBreak);
            }

            if (!string.IsNullOrEmpty(Text))
            {
                _ = sb.Append(WebUtility.HtmlEncode(Text));
                if (!sb.ToString().EndsWith("."))
                {
                    _ = sb.Append('.');
                }
                _ = sb.Append(lineBreak);
            }



            if (!string.IsNullOrEmpty(Power) || !string.IsNullOrEmpty(Toughness))
            {
                _ = sb.Append(string.Format("<b>P/T: {0}/{1}</b>", Power, Toughness));
                _ = sb.Append(lineBreak);
            }

            if (Loyalty.HasValue)
            {
                _ = sb.Append(string.Format("<b>Loyalty:</b> {0}", Loyalty.Value));
                _ = sb.Append(lineBreak);
            }

            if (!string.IsNullOrEmpty(Flavor))
            {
                _ = sb.AppendFormat("<i>{0}</i>", WebUtility.HtmlEncode(Flavor));
                _ = sb.Append(lineBreak);
            }

            if (UseCredits())
            {
                _ = sb.Append($"Revealed by: <a href='{CreditsUrl}'>{Credits}</a>");
                _ = sb.Append(lineBreak);
            }

            _ = sb.Append($"<a href='{FullUrlWebSite}'><i>Link</i></a>");

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
                    _ = sb.Append(Name);
                }

                _ = sb.Append(" #MTG ");

                if (UseCredits())
                {
                    _ = sb.Append($"Revealed by: {CreditsUrl}");
                    _ = sb.Append(lineBreak);
                }

                _ = sb.Append(lineBreak);
                _ = sb.Append(FullUrlWebSite);

                string txt = sb.ToString();
                if (txt.Length > 240)
                {
                    txt = txt[..239];
                }
                return txt;
            }
        }
        public string GetRarityCharacter()
        {
            return Rarity.HasValue
                ? Rarity switch
                {
                    StorageClasses.Rarity.Common => "C",
                    StorageClasses.Rarity.Uncommon => "U",
                    StorageClasses.Rarity.Rare => "R",
                    StorageClasses.Rarity.Mythic => "M",
                    _ => string.Empty,
                }
                : string.Empty;
        }

        public override string ToString()
        {
            return !string.IsNullOrWhiteSpace(Name) ? $"{Name}" : FullUrlWebSite;
        }

        public void AddExtraSide(Card card)
        {
            ExtraSides ??= new List<Card>();
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