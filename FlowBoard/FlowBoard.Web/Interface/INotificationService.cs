namespace FlowBoard.Web.Services
{
    public interface INotificationService
    {
        Task<List<object>> GetByRecipientAsync(int userId, string token);
        Task<bool>         MarkAsReadAsync(int notificationId, string token);
        Task<bool>         MarkAllReadAsync(int userId, string token);
        Task<bool>         DeleteReadAsync(int userId, string token);
        Task<int>          GetUnreadCountAsync(int userId, string token);
        Task<bool>         DeleteAsync(int id, string token);
        Task<bool>         SendBulkAsync(object model, string token);
        Task<List<object>> GetAllAsync(string token);
    }
}