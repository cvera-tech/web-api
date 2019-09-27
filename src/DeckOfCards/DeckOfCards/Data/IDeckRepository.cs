using System.Threading.Tasks;

namespace DeckOfCards.Data
{
    public interface IDeckRepository
    {
        Task<Deck> AddToPileAsync(string deckId, string pileName, string[] cardCodes);
        Task<Deck> CreateNewShuffledDeckAsync(int deckCount);
        Task<Deck> DrawCardsAsync(string deckId, int numCards);
        Task<Deck> GetDeckAsync(string deckId);
        Task<bool> ShufflePileAsync(string deckId, string pileName);
    }
}