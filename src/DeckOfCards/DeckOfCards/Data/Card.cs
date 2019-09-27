using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DeckOfCards.Data
{
    [Table("Card")]
    public class Card
    {
        public int Id { get; set; }

        public bool Drawn { get; set; }

        [Column("CardOrder")]
        public int Order { get; set; }

        [Required]
        [MaxLength(10)]
        public string Value { get; set; }

        [Required]
        [MaxLength(8)]
        public string Suit { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(2)]
        public string Code { get; set; }

        [Required]
        public int DeckId { get; set; }
        public Deck Deck { get; set; }

        public int? PileId { get; set; }
        public Pile Pile { get; set; }
    }
}