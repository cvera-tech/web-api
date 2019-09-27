using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DeckOfCards.Data
{
    public class DeckRepository : IDeckRepository
    {
        async public Task<Deck> CreateNewShuffledDeckAsync(int deckCount)
        {
            var random = new Random();

            var suits = new[] { "HEARTS", "SPADES", "CLUBS", "DIAMONDS" };
            var values = new[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "JACK", "QUEEN", "KING" };
            var cards = new Card[52 * deckCount];
            var deck = new Deck { DeckId = random.Next().ToString("X") };

            int newCardIndex = 0;
            for (int _ = 0; _ < deckCount; _ += 1)
            {
                foreach (string suit in suits)
                {
                    foreach (string value in values)
                    {
                        string code = value.Substring(0, 1) + suit.Substring(0, 1);
                        if (value == "10")
                        {
                            code = "0" + suit.Substring(0, 1);
                        }
                        cards[newCardIndex] = new Card
                        {
                            Deck = deck,
                            Value = value,
                            Suit = suit,
                            Code = code,
                        };
                        newCardIndex += 1;
                    }
                }
            }

            // Fisher-Yates shuffle
            for (int cardIndex = cards.Length - 1; cardIndex >= 0; cardIndex -= 1)
            {
                int swapIndex = random.Next(0, cardIndex);
                Card card = cards[swapIndex];
                cards[swapIndex] = cards[cardIndex];
                cards[cardIndex] = card;
                cards[cardIndex].Order = cardIndex;
                cards[swapIndex].Order = swapIndex;
            }

            foreach (Card card in cards)
            {
                deck.Cards.Add(card);
            }

            using (var context = new DeckContext())
            {
                context.Decks.Add(deck);
                await context.SaveChangesAsync();
            }

            return deck;
        }

        async public Task<Deck> DrawCardsAsync(string deckId, int numCards)
        {
            using (var context = new DeckContext())
            {
                var deck = await context.Decks
                    .Include(d => d.Cards)
                    .SingleAsync(d => d.DeckId == deckId);

                var notDrawn = deck.Cards
                    .Where(c => !c.Drawn)
                    .Take(numCards)
                    .ToList();
                notDrawn.ForEach(c => c.Drawn = !c.Drawn);
                await context.SaveChangesAsync();

                return deck;
            }
        }

        async public Task<Deck> AddToPileAsync(string deckId, string pileName, string[] cardCodes)
        {
            using (var context = new DeckContext())
            {
                var deck = await context.Decks
                    .Include(d => d.Cards)
                    .Include(d => d.Piles)
                    .SingleAsync(d => d.DeckId == deckId);

                // TODO Confirm that cards to add are all drawn

                // Create a new pile if it doesn't exist
                if (!deck.Piles.Select(p => p.Name).Contains(pileName))
                {
                    deck.Piles.Add(new Pile()
                    {
                        Name = pileName,
                        DeckId = deck.Id
                    });
                }

                // Remove cards from other piles
                // I'm not sure if I can just modify the pileIds of each card without 
                // modifying all the piles in the deck.
                var cards = deck.Cards.Where(c => cardCodes.Contains(c.Code)).ToList();
                cards.ForEach(c =>
                {
                    if (c.PileId.HasValue)
                    {
                        deck.Piles.Single(p => p.Id == c.PileId.Value).Cards.Remove(c);
                    }
                });

                // Add cards to pile
                var pile = deck.Piles.Single(p => p.Name == pileName);
                pile.Cards = pile.Cards.Concat(cards).ToList();

                await context.SaveChangesAsync();
                return deck;
            }
        }
    }
}