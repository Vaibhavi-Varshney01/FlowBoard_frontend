using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard.Workspace.Models
{
    [Table("workspaces")]
    public class Workspace
    {
        [Key]
        [Column("workspace_id")]
        public int WorkspaceId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column("owner_id")]
        public string OwnerId { get; set; } = string.Empty;

        [Column("visibility")]
        [MaxLength(10)]
        public string Visibility { get; set; } = "PRIVATE";

        [Column("logo_url")]
        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
    }
}