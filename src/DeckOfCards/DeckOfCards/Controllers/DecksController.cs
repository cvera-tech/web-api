using DeckOfCards.Data;
using DeckOfCards.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace DeckOfCards.Controllers
{
    [RoutePrefix("api/decks")]
    public class DecksController : ApiController
    {
        private IDeckRepository repository;

        public DecksController(IDeckRepository repository)
        {
            this.repository = repository;
        }

        public async Task<ShortDeckInfo> Post(DeckCreate model)
        {
            int count = model.Count.HasValue ? model.Count.Value : 1;
            var deck = await repository.CreateNewShuffledDeckAsync(count);
            var deckInfo = new ShortDeckInfo()
            {
                DeckId = deck.DeckId,
                Remaining = deck.Cards.Where(card => !card.Drawn).Count()
            };
            return deckInfo;
        }

        [Route("{deckId}/cards")]
        public async Task<CardDrawnResponse> Delete (string deckId, CardDrawRequest request)
        {
            int count = request.Count.HasValue ? request.Count.Value : 1;
            var deck = await repository.DrawCardsAsync(deckId, count);
            var drawnCards = deck.Cards
                .Where(c => c.Drawn)
                .Reverse()
                .Take(count)
                .Reverse()
                .Select(c => new CardInfo()
                {
                    Code = c.Code,
                    Suit = c.Suit,
                    Value = c.Value
                })
                .ToList();
            var response = new CardDrawnResponse()
            {
                DeckId = deck.DeckId,
                Remaining = deck.Cards.Where(card => !card.Drawn).Count(),
                Removed = drawnCards
            };
            return response;
        }
    }
}
