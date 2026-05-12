using FlowBoard.Notification.Models;

namespace FlowBoard.Notification.Repositories
{
    public interface INotificationRepository
    {
        // ── Queries ───────────────────────────────────────────────────────────

        Task<List<Models.Notification>> FindByRecipientId(string recipientId);

        Task<List<Models.Notification>> FindByRecipientIdAndIsRead(string recipientId, bool isRead);

        Task<int> CountByRecipientIdAndIsRead(string recipientId, bool isRead);

        Task<List<Models.Notification>> FindByType(NotificationType type);

        Task<List<Models.Notification>> FindByRelatedId(int relatedId);

        Task<Models.Notification?> FindByNotificationId(int notificationId);

        // ── Mutations ─────────────────────────────────────────────────────────

        Task<Models.Notification> Save(Models.Notification notification);

        Task<Models.Notification> Update(Models.Notification notification);

        Task DeleteByNotificationId(int notificationId);

        /// <summary>
        /// Bulk-deletes all read notifications for a given recipient.
        /// Used by the "clear read" feature.
        /// </summary>
        Task DeleteByRecipientIdAndIsRead(string recipientId, bool isRead);
    }
}