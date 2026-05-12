using FlowBoard.Comment.Models;

namespace FlowBoard.Comment.Repositories
{
    public interface ICommentRepository
    {
        // Comment queries
        Task<List<Models.Comment>> FindByCardId(int cardId);
        Task<List<Models.Comment>> FindByAuthorId(string authorId);
        Task<Models.Comment?> FindByCommentId(int commentId);
        Task<List<Models.Comment>> FindByParentCommentId(int parentCommentId);
        Task<int> CountByCardId(int cardId);
        Task DeleteByCommentId(int commentId);

        Task<Models.Comment> Save(Models.Comment comment);
        Task<Models.Comment> Update(Models.Comment comment);

        // Attachment queries
        Task<List<Attachment>> FindAttachmentsByCardId(int cardId);
        Task<Attachment?> FindAttachmentById(int attachmentId);
        Task<Attachment> SaveAttachment(Attachment attachment);
        Task DeleteAttachment(int attachmentId);
    }
}