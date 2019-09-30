using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DeckOfCards.Data
{
    public class DeckContext : DbContext
    {
        public DbSet<Card> Cards { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<Pile> Piles { get; set; }

        public DeckContext() : base("name=DeckOfCardsConnection")
        {
            Database.SetInitializer<DeckContext>(null);
        }
    }
}