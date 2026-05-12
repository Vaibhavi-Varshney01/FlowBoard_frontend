using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard.Checklist.Models
{
    [Table("labels")]
    public class Label
    {
        [Key]
        [Column("label_id")]
        public int LabelId { get; set; }

        [Required]
        [Column("board_id")]
        public int BoardId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("color")]
        [MaxLength(7)]
        public string Color { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}