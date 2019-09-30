using System.Collections.Generic;

namespace DeckOfCards.Models
{
    public class CardDrawnResponse
    {
        public string DeckId { get; set; }

        public int Remaining { get; set; }

        public List<CardInfo> Removed { get; set; }

        public CardDrawnResponse()
        {
            Removed = new List<CardInfo>();
        }
    }
}