using System.Collections.Generic;

namespace DeckOfCards.Models
{
    public class PileInfo : IPileInfo
    {
        public List<CardInfo> Cards { get; set; }

        public int Remaining { get; set; }

        public PileInfo()
        {
            Cards = new List<CardInfo>();
        }
    }
}