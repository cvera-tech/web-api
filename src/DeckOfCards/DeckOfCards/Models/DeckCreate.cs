namespace DeckOfCards.Models
{
    public class DeckCreate
    {
        public int? Count { get; set; }

        public DeckCreate()
        {
            Count = 1;
        }
    }
}