namespace FlowBoard.Web.Services
{
    public interface ICommentService
    {
        Task<List<object>> GetByCardAsync(int cardId, string token);
        Task<object?>      AddCommentAsync(int cardId, string content, string token);
        Task<bool>         EditCommentAsync(int id, string content, string token);
        Task<bool>         DeleteCommentAsync(int id, string token);
        Task<List<object>> GetRepliesAsync(int commentId, string token);
        Task<bool>         AddAttachmentAsync(int cardId, IFormFile file, string token);
        Task<bool>         DeleteAttachmentAsync(int id, string token);
        Task<List<object>> GetAttachmentsByCardAsync(int cardId, string token);
    }
}