using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard.Board.Models
{
    [Table("boards")]
    public class Board
    {
        [Key]
        [Column("board_id")]
        public int BoardId { get; set; }

        [Required]
        [Column("workspace_id")]
        public int WorkspaceId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Column("background")]
        [MaxLength(200)]
        public string? Background { get; set; }

        [Column("visibility")]
        [MaxLength(10)]
        public string Visibility { get; set; } = "PRIVATE";

        [Required]
        [Column("created_by_id")]
        public string CreatedById { get; set; } = string.Empty;

        [Column("is_closed")]
        public bool IsClosed { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<BoardMember> Members { get; set; } = new List<BoardMember>();
    }
}