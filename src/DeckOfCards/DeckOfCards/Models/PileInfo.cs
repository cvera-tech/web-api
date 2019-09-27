using DeckOfCards.Data;
using System.Collections.Generic;

namespace DeckOfCards.Models
{
    public class PileInfo : IPileInfo
    {
        public List<Card> Cards { get; set; }

        public int Remaining { get; set; }
    }
}