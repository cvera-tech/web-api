using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DeckOfCards.Data
{
    [Table("Deck")]
    public class Deck
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string DeckId { get; set; }

        public IList<Pile> Piles { get; set; }

        public IList<Card> Cards { get; set; }

        public int Remaining { get { return Cards.Where(c => !c.Drawn).Count(); } }

        public Deck()
        {
            Piles = new List<Pile>();
            Cards = new List<Card>();
        }
    }
}