using FlowBoard.Notification.Infrastructure;
using FlowBoard.Notification.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Notification.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _db;

        public NotificationRepository(NotificationDbContext db)
        {
            _db = db;
        }

        // ── Queries ───────────────────────────────────────────────────────────

        public async Task<List<Models.Notification>> FindByRecipientId(string recipientId)
            => await _db.Notifications
                        .Where(n => n.RecipientId == recipientId || n.RecipientId == "ALL")
                        .OrderByDescending(n => n.CreatedAt)
                        .ToListAsync();

        public async Task<List<Models.Notification>> FindByRecipientIdAndIsRead(
            string recipientId, bool isRead)
            => await _db.Notifications
                        .Where(n => n.RecipientId == recipientId && n.IsRead == isRead)
                        .OrderByDescending(n => n.CreatedAt)
                        .ToListAsync();

        public async Task<int> CountByRecipientIdAndIsRead(string recipientId, bool isRead)
            => await _db.Notifications
                        .CountAsync(n => n.RecipientId == recipientId && n.IsRead == isRead);

        public async Task<List<Models.Notification>> FindByType(NotificationType type)
            => await _db.Notifications
                        .Where(n => n.Type == type)
                        .OrderByDescending(n => n.CreatedAt)
                        .ToListAsync();

        public async Task<List<Models.Notification>> FindByRelatedId(int relatedId)
            => await _db.Notifications
                        .Where(n => n.RelatedId == relatedId)
                        .OrderByDescending(n => n.CreatedAt)
                        .ToListAsync();

        public async Task<Models.Notification?> FindByNotificationId(int notificationId)
            => await _db.Notifications.FindAsync(notificationId);

        // ── Mutations ─────────────────────────────────────────────────────────

        public async Task<Models.Notification> Save(Models.Notification notification)
        {
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
            return notification;
        }

        public async Task<Models.Notification> Update(Models.Notification notification)
        {
            _db.Notifications.Update(notification);
            await _db.SaveChangesAsync();
            return notification;
        }

        public async Task DeleteByNotificationId(int notificationId)
        {
            var entity = await _db.Notifications.FindAsync(notificationId);
            if (entity is not null)
            {
                _db.Notifications.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteByRecipientIdAndIsRead(string recipientId, bool isRead)
        {
            var items = await _db.Notifications
                                 .Where(n => n.RecipientId == recipientId && n.IsRead == isRead)
                                 .ToListAsync();

            if (items.Count == 0) return;

            _db.Notifications.RemoveRange(items);
            await _db.SaveChangesAsync();
        }
    }
}