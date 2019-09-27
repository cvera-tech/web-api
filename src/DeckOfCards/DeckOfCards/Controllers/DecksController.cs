using DeckOfCards.Data;
using DeckOfCards.Models;
using System.Collections.Generic;
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

        [Route("{deckId}/piles/{pileName}")]
        public async Task<AddToPileResponse> Patch(string deckId, string pileName, AddToPileRequest request)
        {
            var deck = await repository.AddToPileAsync(deckId, pileName, request.CardCodes);
            var dictionary = new Dictionary<string, PileInfo>();
            deck.Piles
                .ToList()
                .ForEach(p => dictionary.Add(p.Name, new PileInfo() { Remaining = p.Remaining }));
            var response = new AddToPileResponse()
            {
                DeckId = deck.DeckId,
                Remaining = deck.Remaining,
                Piles = dictionary
            };
            return response;
        }

        [Route("{deckId}/cards")]
        public async Task<CardDrawnResponse> Delete(string deckId, CardDrawRequest request)
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
