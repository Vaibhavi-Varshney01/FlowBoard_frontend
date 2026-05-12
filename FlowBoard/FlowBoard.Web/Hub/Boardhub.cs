using Microsoft.AspNetCore.SignalR;

namespace FlowBoard.Web.Hubs
{
    // Real-time board updates — replaces Spring WebSocket/STOMP
    // Broadcasts card moves, comment additions, and checklist toggles
    // to all active board viewers in real time
    public class BoardHub : Hub
    {
        // Join a board group to receive real-time updates for that board
        public async Task JoinBoard(string boardId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"board-{boardId}");
        }

        // Leave a board group when user navigates away
        public async Task LeaveBoard(string boardId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board-{boardId}");
        }

        // Called by server to notify all viewers of a card move
        public async Task NotifyCardMoved(string boardId, object cardData)
        {
            await Clients.Group($"board-{boardId}").SendAsync("CardMoved", cardData);
        }

        // Called by server to notify all viewers of a new comment
        public async Task NotifyCommentAdded(string boardId, object commentData)
        {
            await Clients.Group($"board-{boardId}").SendAsync("CommentAdded", commentData);
        }

        // Called by server to notify all viewers of a checklist toggle
        public async Task NotifyChecklistToggled(string boardId, object itemData)
        {
            await Clients.Group($"board-{boardId}").SendAsync("ChecklistToggled", itemData);
        }

        // Update unread notification badge count for a specific user
        public async Task NotifyUnreadCount(string userId, int count)
        {
            await Clients.User(userId).SendAsync("UnreadCountUpdated", count);
        }
    }
}