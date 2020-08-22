using System;
namespace MagicNewCardsBot
{
    public class Set
    {
        public Int64 ID { get; set; }
        public string URL { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Name} - {URL}";
        }

    }
}