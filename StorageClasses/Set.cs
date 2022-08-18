using System;
namespace MagicNewCardsBot.StorageClasses
{
    public class Set
    {
        public long ID { get; set; }
        public string URL { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Name} - {URL}";
        }

    }
}