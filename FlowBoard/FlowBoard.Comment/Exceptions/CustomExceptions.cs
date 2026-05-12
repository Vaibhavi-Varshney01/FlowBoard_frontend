namespace FlowBoard.Comment.Exceptions
{
    public class CommentNotFoundException : Exception
    {
        public CommentNotFoundException(int commentId)
            : base($"Comment {commentId} not found.") { }
    }

    public class CommentDeletedException : Exception
    {
        public CommentDeletedException(int commentId)
            : base($"Comment {commentId} has been deleted.") { }
    }

    public class AttachmentNotFoundException : Exception
    {
        public AttachmentNotFoundException(int attachmentId)
            : base($"Attachment {attachmentId} not found.") { }
    }

    public class UnauthorizedCommentEditException : Exception
    {
        public UnauthorizedCommentEditException()
            : base("Only the author can edit or delete this comment.") { }
    }

    public class UnauthorizedAttachmentDeleteException : Exception
    {
        public UnauthorizedAttachmentDeleteException()
            : base("Only the uploader can delete this attachment.") { }
    }

    public class InvalidReplyException : Exception
    {
        public InvalidReplyException(string message)
            : base(message) { }
    }

    public class S3UploadException : Exception
    {
        public S3UploadException(string message)
            : base($"S3 upload failed: {message}") { }
    }
}