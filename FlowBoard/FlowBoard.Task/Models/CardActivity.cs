using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard.Card.Models
{
    [Table("card_activities")]
    public class CardActivity
    {
        [Key]
        [Column("activity_id")]
        public int ActivityId { get; set; }

        [Required]
        [Column("card_id")]
        public int CardId { get; set; }

        [Required]
        [Column("actor_id")]
        public string ActorId { get; set; } = string.Empty;

        [Required]
        [Column("action")]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Column("detail")]
        public string? Detail { get; set; }

        [Column("occurred_at")]
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        // Navigation (read-only, not updated after insert)
        [ForeignKey("CardId")]
        public Card? Card { get; set; }
    }
}