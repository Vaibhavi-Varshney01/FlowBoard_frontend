using FlowBoard.Notification.Models;

namespace FlowBoard.Notification.Events
{
    // ── Published by this service ─────────────────────────────────────────────

    /// <summary>
    /// Fan-out event for admin broadcasts / system reminders.
    /// Published by NotificationServiceImpl, consumed by BulkNotificationConsumer.
    /// </summary>
    public class BulkNotificationEvent
    {
        public List<string>      RecipientIds { get; set; } = new();
        public string            ActorId      { get; set; } = string.Empty;
        public NotificationType  Type         { get; set; }
        public string            Message      { get; set; } = string.Empty;
        public string            Title        { get; set; } = string.Empty;
        public int?              RelatedId    { get; set; }
        public string?           RelatedType  { get; set; }
        public DateTime          OccurredAt   { get; set; } = DateTime.UtcNow;
    }

    // ── Consumed from other services ──────────────────────────────────────────

    /// <summary>
    /// Published by Task-Service when a card is assigned to a user.
    /// </summary>
    public class CardAssignedEvent
    {
        public int    CardId      { get; set; }
        public string CardTitle   { get; set; } = string.Empty;
        public string AssigneeId  { get; set; } = string.Empty;
        public string ActorId     { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Published by Comment-Service when a user is @mentioned in a comment.
    /// </summary>
    public class MentionEvent
    {
        public int    CommentId   { get; set; }
        public int    CardId      { get; set; }
        public string MentionedId { get; set; } = string.Empty;  // recipient
        public string ActorId     { get; set; } = string.Empty;
        public string Excerpt     { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Published by Comment-Service when a reply is posted.
    /// </summary>
    public class CommentReplyEvent
    {
        public int    CommentId       { get; set; }
        public int    ParentCommentId { get; set; }
        public int    CardId          { get; set; }
        public string ParentAuthorId  { get; set; } = string.Empty;  // recipient
        public string ActorId         { get; set; } = string.Empty;
        public string Excerpt         { get; set; } = string.Empty;
        public DateTime OccurredAt    { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Published by Task-Service when a card is moved to the Done column.
    /// </summary>
    public class CardMovedToDoneEvent
    {
        public int    CardId      { get; set; }
        public string CardTitle   { get; set; } = string.Empty;
        public string ActorId     { get; set; } = string.Empty;
        public string RecipientId { get; set; } = string.Empty;  // card creator / assignee
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Published by Quartz.NET scheduler (DueDateReminderJob) 1 day and 1 hour before due.
    /// </summary>
    public class DueDateReminderEvent
    {
        public int    CardId      { get; set; }
        public string CardTitle   { get; set; } = string.Empty;
        public string AssigneeId  { get; set; } = string.Empty;  // recipient
        public DateTime DueAt     { get; set; }
        public string Horizon     { get; set; } = string.Empty;  // "1 day" | "1 hour"
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}