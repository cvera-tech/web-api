using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DeckOfCards.Data
{
    public class DeckRepository : IDeckRepository
    {
        /// <summary>
        /// Puts a collection of cards of a given deck in a pile.
        /// </summary>
        /// <param name="deckId">The ID of the deck.</param>
        /// <param name="pileName">The name of the pile.</param>
        /// <param name="cardCodes">The codes of the cards to put in the pile.</param>
        /// <returns>The updated deck.</returns>
        public async Task<Deck> AddToPileAsync(string deckId, string pileName, string[] cardCodes)
        {
            using (var context = new DeckContext())
            {
                var deck = await context.Decks
                    .Include(d => d.Cards)
                    .Include(d => d.Piles)
                    .SingleAsync(d => d.DeckId == deckId);

                // TODO Confirm that cards to add are all drawn

                // I'm not sure how to best format these LINQ statements
                if (!deck.Piles
                    .Select(p => p.Name)
                    .Contains(pileName))
                {
                    deck.Piles.Add(new Pile()
                    {
                        Name = pileName,
                        DeckId = deck.Id
                    });
                }

                // I don't know if I can just modify the pileIds of each card without 
                // modifying all the piles in the deck.
                //
                // This will also break when a deck has multiple copies of the same card
                // (e.g. when the CreateNewShuffledDeckAsync method is passed an int > 1)
                // because I'm filtering by card code instead of card id.
                //
                // Perhaps this can be fixed if we require a "source" field in the request.
                var cards = deck.Cards
                    .Where(c => cardCodes.Contains(c.Code))
                    .ToList();

                cards.ForEach(c =>
                {
                    if (c.PileId.HasValue)
                    {
                        deck.Piles
                            .Single(p => p.Id == c.PileId.Value)
                            .Cards
                            .Remove(c);
                    }
                });

                var pile = deck.Piles.Single(p => p.Name == pileName);
                pile.Cards = pile.Cards.Concat(cards).ToList();
                await context.SaveChangesAsync();
                return deck;
            }
        }

        /// <summary>
        /// Creates a new deck and shuffles its cards.
        /// </summary>
        /// <param name="deckCount">The number of sets of cards to populate the deck with.</param>
        /// <returns>The created deck.</returns>
        public async Task<Deck> CreateNewShuffledDeckAsync(int deckCount)
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

            // Fisher - Yates shuffle
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

        /// <summary>
        /// Draws a number of cards from a given deck.
        /// </summary>
        /// <param name="deckId">The ID of the deck.</param>
        /// <param name="numCards">The number of cards to draw.</param>
        /// <returns>The updated deck.</returns>
        public async Task<Deck> DrawCardsAsync(string deckId, int numCards)
        {
            using (var context = new DeckContext())
            {
                var deck = await context.Decks
                    .Include(d => d.Cards)
                    .SingleAsync(d => d.DeckId == deckId);

                // TODO handle the case when there are not enough cards to draw
                var notDrawn = deck.Cards
                    .Where(c => !c.Drawn)
                    .Take(numCards)
                    .ToList();
                notDrawn.ForEach(c => c.Drawn = !c.Drawn);
                await context.SaveChangesAsync();

                return deck;
            }
        }

        /// <summary>
        /// Shuffles the cards in a list.
        /// </summary>
        /// <param name="cards">The list of cards to shuffle.</param>
        private void Shuffle(List<Card> cards)
        {
            var random = new Random();
            var cardOrders = cards.Select(c => c.Order).ToList();

            // Fisher-Yates shuffle
            for (int cardIndex = cardOrders.Count - 1; cardIndex >= 0; cardIndex -= 1)
            {
                int swapIndex = random.Next(0, cardIndex);
                int swapValue = cardOrders[swapIndex];
                cardOrders[swapIndex] = cardOrders[cardIndex];
                cardOrders[cardIndex] = swapValue;
            }

            // Reassign card order
            for (int index = 0; index < cardOrders.Count; index += 1)
            {
                cards[index].Order = cardOrders[index];
            }
        }

        /// <summary>
        /// Shuffles the cards of a pile in a given deck.
        /// </summary>
        /// <param name="deckId">The ID of the deck.</param>
        /// <param name="pileName">The name of the pile.</param>
        /// <returns>True if the shuffle was successful; false otherwise.</returns>
        public async Task<bool> ShufflePileAsync(string deckId, string pileName)
        {
            using (var context = new DeckContext())
            {
                var deck = await context.Decks
                    .Include(d => d.Cards)
                    .Include(d => d.Piles)
                    .SingleAsync(d => d.DeckId == deckId);
                var piles = deck.Piles;
                var pile = piles.Single(p => p.Name == pileName);
                var cards = pile.Cards.ToList();

                try
                {
                    Shuffle(cards);
                    pile.Cards = cards;
                    await context.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task<Deck> GetDeckAsync(string deckId)
        {
            throw new NotImplementedException();
        }
    }
}