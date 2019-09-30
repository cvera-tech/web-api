namespace DeckOfCards.Models
{
    public class CardDrawRequest
    {
        public int? Count { get; set; }

        public CardDrawRequest()
        {
            Count = 1;
        }
    }
}