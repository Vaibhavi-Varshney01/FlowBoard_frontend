using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard.Checklist.Models
{
    [Table("checklist_items")]
    public class ChecklistItem
    {
        [Key]
        [Column("item_id")]
        public int ItemId { get; set; }

        [Required]
        [Column("checklist_id")]
        public int ChecklistId { get; set; }

        [Required]
        [Column("text")]
        [MaxLength(500)]
        public string Text { get; set; } = string.Empty;

        [Column("is_completed")]
        public bool IsCompleted { get; set; } = false;

        [Column("assignee_id")]
        public int? AssigneeId { get; set; }

        [Column("due_date")]
        public DateOnly? DueDate { get; set; }
    }
}