using System.Collections.Generic;

namespace DeckOfCards.Models
{
    public class ListPileResponse
    {
        public string DeckId { get; set; }
        public int Remaining { get; set; }
        public Dictionary<string, IPileInfo> Piles { get; set; }
    }
}