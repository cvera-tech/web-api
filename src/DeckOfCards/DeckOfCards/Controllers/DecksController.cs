using DeckOfCards.Data;
using DeckOfCards.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace DeckOfCards.Controllers
{
    [RoutePrefix("api")]
    public class DecksController : ApiController
    {
        private IDeckRepository repository;

        public DecksController(IDeckRepository repository)
        {
            this.repository = repository;
        }

        [Route("decks")]
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

        [Route("shuffler")]
        public async Task<HttpStatusCode> Post(ShufflePileRequest model)
        {
            var result = await repository.ShufflePileAsync(model.DeckId, model.Pile);
            // Requirements says we should return 201 (Created), but I think
            // 204 (No Content) is more appropriate for shuffling.
            // I also don't know which code to return on fail.
            return result ? HttpStatusCode.NoContent : HttpStatusCode.Conflict;
        }

        [Route("decks/{deckId}/piles/{pileName}")]
        public async Task<AddToPileResponse> Patch(string deckId, string pileName, AddToPileRequest request)
        {
            var deck = await repository.AddToPileAsync(deckId, pileName, request.CardCodes);
            var dictionary = new Dictionary<string, ShortPileInfo>();
            deck.Piles
                .ToList()
                .ForEach(p => dictionary.Add(p.Name, new ShortPileInfo() { Remaining = p.Remaining }));
            var response = new AddToPileResponse()
            {
                DeckId = deck.DeckId,
                Remaining = deck.Remaining,
                Piles = dictionary
            };
            return response;
        }

        [Route("decks/{deckId}/cards")]
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
