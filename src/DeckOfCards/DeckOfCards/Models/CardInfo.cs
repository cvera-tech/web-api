namespace DeckOfCards.Models
{
    public class CardInfo
    {
        public string Code { get; set; }

        public string Suit { get; set; }

        public string Value { get; set; }

        public string Image
        {
            get
            {
                return $"https://deckofcardsapi.com/static/img/{Code}.png";
            }
        }
    }
}