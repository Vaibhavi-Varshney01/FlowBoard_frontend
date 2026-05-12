namespace FlowBoard.Comment.Events
{
    // Published when a new comment is added — consumed by Notification Service
    public class CommentAddedEvent
    {
        public int    CommentId  { get; set; }
        public int    CardId     { get; set; }
        public string AuthorId   { get; set; } = string.Empty;
        public string Content    { get; set; } = string.Empty;
        public int?   ParentCommentId { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    // Published when an attachment is uploaded — consumed by Notification Service
    public class AttachmentAddedEvent
    {
        public int    AttachmentId { get; set; }
        public int    CardId       { get; set; }
        public string UploaderId   { get; set; } = string.Empty;
        public string FileName     { get; set; } = string.Empty;
        public string FileUrl      { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}