using FlowBoard.Notification.Models;

namespace FlowBoard.Notification.Services
{
    // ── Request Records ────────────────────────────────────────────────────────

    public record SendNotificationRequest(
        string           RecipientId,
        string           ActorId,
        NotificationType Type,
        string           Message,
        string           Title,
        int?             RelatedId   = null,
        string?          RelatedType = null
    );

    public record SendBulkNotificationRequest(
        List<string>     RecipientIds,
        string           ActorId,
        NotificationType Type,
        string           Message,
        string           Title,
        int?             RelatedId   = null,
        string?          RelatedType = null
    );

    public record SendEmailRequest(
        string ToEmail,
        string Subject,
        string Body
    );

    public record BroadcastRequest(string Message);

    // ── Interface ──────────────────────────────────────────────────────────────

    public interface INotificationService
    {
        // ── Dispatch ──────────────────────────────────────────────────────────

        /// <summary>Saves and pushes a single in-app notification via SignalR.</summary>
        Task<Models.Notification> Send(SendNotificationRequest request);

        /// <summary>
        /// Sends the same notification to multiple recipients (admin broadcast /
        /// system reminders). Published via MassTransit for fan-out.
        /// </summary>
        Task SendBulk(SendBulkNotificationRequest request);

        // ── Read-state management ─────────────────────────────────────────────

        Task MarkAsRead(int notificationId);

        Task MarkAllRead(string recipientId);

        Task DeleteRead(string recipientId);

        // ── Retrieval ─────────────────────────────────────────────────────────

        Task<List<Models.Notification>> GetByRecipient(string recipientId);

        Task<int> GetUnreadCount(string recipientId);

        Task DeleteNotification(int notificationId);

        // ── Email ─────────────────────────────────────────────────────────────

        /// <summary>Sends a transactional email via SendGrid .NET SDK.</summary>
        Task SendEmail(string toEmail, string subject, string body);

        // ── Admin / debug ─────────────────────────────────────────────────────

        Task<List<Models.Notification>> GetAll();

        /// <summary>Broadcast to all connected users via SignalR.</summary>
        Task Broadcast(string message);
    }
}