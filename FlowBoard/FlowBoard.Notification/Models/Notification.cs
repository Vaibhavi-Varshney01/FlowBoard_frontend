using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard.Notification.Models
{
    public enum NotificationType
    {
        ASSIGNMENT,
        MENTION,
        DUE_DATE,
        COMMENT,
        MOVE,
        SYSTEM_BROADCAST
    }

    [Table("notifications")]
    public class Notification
    {
        [Key]
        [Column("notification_id")]
        public int NotificationId { get; set; }

        [Required]
        [Column("recipient_id")]
        public string RecipientId { get; set; } = string.Empty;

        [Required]
        [Column("actor_id")]
        public string ActorId { get; set; } = string.Empty;

        [Required]
        [Column("type")]
        public NotificationType Type { get; set; }

        [Required]
        [Column("message")]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Column("title")]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        // CardId or BoardId
        [Column("related_id")]
        public int? RelatedId { get; set; }

        // "Card" or "Board" — used for deep-linking on the frontend
        [Column("related_type")]
        [MaxLength(50)]
        public string? RelatedType { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Convenience constructors ───────────────────────────────────────────

        public Notification() { }

        public int GetNotificationId() => NotificationId;
        public string GetRecipientId() => RecipientId;
        public string GetActorId()     => ActorId;
        public NotificationType GetType() => Type;
        public string GetMessage()     => Message;
        public string GetTitle()       => Title;
        public int? GetRelatedId()     => RelatedId;
        public string? GetRelatedType() => RelatedType;
        public bool IsRead_()          => IsRead;
        public DateTime GetCreatedAt() => CreatedAt;

        public void SetRead(bool value) => IsRead = value;

        public override string ToString()
            => $"Notification[{NotificationId}] {Type} -> {RecipientId} | {Title}";
    }
}