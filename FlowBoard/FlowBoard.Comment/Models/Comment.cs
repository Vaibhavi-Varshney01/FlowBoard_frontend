using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard.Comment.Models
{
    [Table("comments")]
    public class Comment
    {
        [Key]
        [Column("comment_id")]
        public int CommentId { get; set; }

        [Required]
        [Column("card_id")]
        public int CardId { get; set; }

        [Required]
        [Column("author_id")]
        public string AuthorId { get; set; } = string.Empty;

        [Required]
        [Column("content")]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        // null = top-level comment, value = reply
        [Column("parent_comment_id")]
        public int? ParentCommentId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft-delete — EF Core query filter hides deleted comments automatically
        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}