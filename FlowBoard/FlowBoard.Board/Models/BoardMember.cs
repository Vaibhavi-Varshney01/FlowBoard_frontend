using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard.Board.Models
{
    [Table("board_members")]
    public class BoardMember
    {
        [Key]
        [Column("board_member_id")]
        public int BoardMemberId { get; set; }

        [Required]
        [Column("board_id")]
        public int BoardId { get; set; }

        [Required]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("role")]
        [MaxLength(20)]
        public string Role { get; set; } = "MEMBER";

        [Column("added_at")]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("BoardId")]
        public Board? Board { get; set; }
    }
}