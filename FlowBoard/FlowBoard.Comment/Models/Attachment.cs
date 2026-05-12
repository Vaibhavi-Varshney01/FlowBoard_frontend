using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard.Comment.Models
{
    [Table("attachments")]
    public class Attachment
    {
        [Key]
        [Column("attachment_id")]
        public int AttachmentId { get; set; }

        [Required]
        [Column("card_id")]
        public int CardId { get; set; }

        [Required]
        [Column("uploader_id")]
        public string UploaderId { get; set; } = string.Empty;

        [Required]
        [Column("file_name")]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        // AWS S3 / Azure Blob public URL
        [Required]
        [Column("file_url")]
        [MaxLength(1000)]
        public string FileUrl { get; set; } = string.Empty;

        [Column("file_type")]
        [MaxLength(50)]
        public string? FileType { get; set; }

        [Column("size_kb")]
        public long SizeKb { get; set; }

        [Column("uploaded_at")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}