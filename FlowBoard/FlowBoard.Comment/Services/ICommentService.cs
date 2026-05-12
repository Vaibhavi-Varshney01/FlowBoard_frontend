using FlowBoard.Comment.Models;
using Microsoft.AspNetCore.Http;

namespace FlowBoard.Comment.Services
{
    // ── Request Records ───────────────────────────────────────────────────────

    public record AddCommentRequest(
        int    CardId,
        string AuthorId,
        string Content,
        int?   ParentCommentId  // null = top-level, value = reply
    );

    public record UpdateCommentRequest(
        string Content
    );

    public record AddAttachmentRequest(
        int    CardId,
        string UploaderId,
        IFormFile File   // actual file upload — S3 mein jayega
    );

    // ── Interface ─────────────────────────────────────────────────────────────

    public interface ICommentService
    {
        // Comment CRUD
        Task<Models.Comment> AddComment(AddCommentRequest request);
        Task<Models.Comment> GetCommentById(int commentId);
        Task<List<Models.Comment>> GetByCard(int cardId);
        Task<List<Models.Comment>> GetReplies(int parentCommentId);
        Task<Models.Comment> UpdateComment(int commentId, UpdateCommentRequest request, string requestingUserId);
        Task DeleteComment(int commentId, string requestingUserId);
        Task<int> GetCommentCount(int cardId);

        // Attachment — file S3 pe upload hoti hai
        Task<Attachment> AddAttachment(AddAttachmentRequest request);
        Task<List<Attachment>> GetAttachmentsByCard(int cardId);
        Task DeleteAttachment(int attachmentId, string requestingUserId);
    }
}