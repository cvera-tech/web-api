using System.Collections.Generic;

namespace DeckOfCards.Models
{
    public class AddToPileResponse
    {
        public string DeckId { get; set; }
        public int Remaining { get; set; }
        public Dictionary<string, PileInfo> Piles { get; set; }
    }
}