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
    }
}
