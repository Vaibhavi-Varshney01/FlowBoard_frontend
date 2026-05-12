using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard.List.Models
{
    [Table("task_lists")]
    public class TaskList
    {
        [Key]
        [Column("list_id")]
        public int ListId { get; set; }

        [Required]
        [Column("board_id")]
        public int BoardId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("position")]
        public int Position { get; set; }

        [Column("color")]
        [MaxLength(20)]
        public string? Color { get; set; }

        [Column("is_archived")]
        public bool IsArchived { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}