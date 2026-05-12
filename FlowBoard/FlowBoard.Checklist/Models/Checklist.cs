using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard.Checklist.Models
{
    [Table("checklists")]
    public class TaskChecklist
    {
        [Key]
        [Column("checklist_id")]
        public int ChecklistId { get; set; }

        [Required]
        [Column("card_id")]
        public int CardId { get; set; }

        [Required]
        [Column("titre")]
        [MaxLength(200)]
        public string Titre { get; set; } = string.Empty;

        [Required]
        [Column("position")]
        public int Position { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}