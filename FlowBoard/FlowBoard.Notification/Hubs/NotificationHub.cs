using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FlowBoard.Notification.Hubs
{
    /// <summary>
    /// Real-time notification hub.
    /// Each authenticated user joins a group named after their UserId so the
    /// server can push targeted notifications without broadcasting to everyone.
    ///
    /// Client subscribes with:
    ///   connection.on("ReceiveNotification", (notification) => { ... })
    ///   connection.on("UnreadCount",          (count)        => { ... })
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                // Join personal group so the service can push to this user
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                _logger.LogInformation(
                    "[NotificationHub] User {UserId} connected — joined group.", userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                _logger.LogInformation(
                    "[NotificationHub] User {UserId} disconnected.", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}