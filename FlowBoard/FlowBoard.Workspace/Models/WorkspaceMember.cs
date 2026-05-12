using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard.Workspace.Models
{
    [Table("workspace_members")]
    public class WorkspaceMember
    {
        [Key]
        [Column("member_id")]
        public int MemberId { get; set; }

        [Required]
        [Column("workspace_id")]
        public int WorkspaceId { get; set; }

        [Required]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("role")]
        [MaxLength(20)]
        public string Role { get; set; } = "MEMBER";

        [Column("joined_at")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("WorkspaceId")]
        public Workspace? Workspace { get; set; }
    }
}