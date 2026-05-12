using FlowBoard.Notification.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowBoard.Notification.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notifService;

        public NotificationController(INotificationService notifService)
        {
            _notifService = notifService;
        }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new UnauthorizedAccessException("User not authenticated.");

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health() => Ok(new { status = "Healthy", service = "Notification" });

        // ── GET /api/notifications/my ─────────────────────────────────────────
        [HttpGet("my")]
        public async Task<ActionResult<List<Models.Notification>>> GetMyNotifications()
        {
            var userId = GetUserId();
            var notifications = await _notifService.GetByRecipient(userId);
            return Ok(notifications);
        }

        // ── GET /api/notifications/recipient/{recipientId} ────────────────────
        [HttpGet("recipient/{recipientId}")]
        public async Task<ActionResult<List<Models.Notification>>> GetByRecipient(string recipientId)
        {
            var notifications = await _notifService.GetByRecipient(recipientId);
            return Ok(notifications);
        }

        // ── PUT /api/notifications/{id}/read ──────────────────────────────────
        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notifService.MarkAsRead(id);
            return NoContent();
        }

        // ── PUT /api/notifications/read-all ───────────────────────────────────
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = GetUserId();
            await _notifService.MarkAllRead(userId);
            return NoContent();
        }

        // ── DELETE /api/notifications/read ────────────────────────────────────
        [HttpDelete("read")]
        public async Task<IActionResult> DeleteRead()
        {
            var userId = GetUserId();
            await _notifService.DeleteRead(userId);
            return NoContent();
        }

        // ── GET /api/notifications/unread-count ───────────────────────────────
        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var userId = GetUserId();
            var count  = await _notifService.GetUnreadCount(userId);
            return Ok(new { recipientId = userId, unreadCount = count });
        }

        // ── DELETE /api/notifications/{id} ────────────────────────────────────
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _notifService.DeleteNotification(id);
            return NoContent();
        }

        // ── POST /api/notifications/bulk ──────────────────────────────────────
        [HttpPost("bulk")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> SendBulk([FromBody] SendBulkNotificationRequest request)
        {
            await _notifService.SendBulk(request);
            return Accepted(new { message = "Bulk notification enqueued.", count = request.RecipientIds.Count });
        }

        // ── POST /api/notifications/broadcast ─────────────────────────────────
        [HttpPost("broadcast")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastRequest request)
        {
            await _notifService.Broadcast(request.Message);
            return Ok(new { message = "Broadcast sent successfully" });
        }

        // ── GET /api/notifications ────────────────────────────────────────────
        [HttpGet("")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<List<Models.Notification>>> GetAll()
        {
            var all = await _notifService.GetAll();
            return Ok(all);
        }
    }
}