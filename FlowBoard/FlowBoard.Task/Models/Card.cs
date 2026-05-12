using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard.Card.Models
{
    public enum Priority
    {
        LOW,
        MEDIUM,
        HIGH,
        CRITICAL
    }

    public enum CardStatus
    {
        TO_DO,
        IN_PROGRESS,
        IN_REVIEW,
        DONE
    }

    [Table("cards")]
    public class Card
    {
        [Key]
        [Column("card_id")]
        public int CardId { get; set; }

        [Required]
        [Column("list_id")]
        public int ListId { get; set; }

        [Required]
        [Column("board_id")]
        public int BoardId { get; set; }

        [Required]
        [Column("title")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("position")]
        public int Position { get; set; }

        [Column("priority")]
        public Priority Priority { get; set; } = Priority.MEDIUM;

        [Column("status")]
        public CardStatus Status { get; set; } = CardStatus.TO_DO;

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("assignee_id")]
        public string? AssigneeId { get; set; }

        [Required]
        [Column("created_by_id")]
        public string CreatedById { get; set; } = string.Empty;

        [Column("is_archived")]
        public bool IsArchived { get; set; } = false;

        [Column("cover_color")]
        [MaxLength(20)]
        public string? CoverColor { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}