using FlowBoard.Comment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowBoard.Comment.Controllers
{
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // JWT se current userId nikalo
        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new UnauthorizedAccessException("User not authenticated.");

        // ── Comment Endpoints ─────────────────────────────────────────────────

        // POST /api/comments
        [HttpPost("api/comments")]
        public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
        {
            var comment = await _commentService.AddComment(request);
            return Ok(comment);
        }

        // GET /api/comments/{id}
        [HttpGet("api/comments/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comment = await _commentService.GetCommentById(id);
            return Ok(comment);
        }

        // GET /api/comments/card/{cardId}
        [HttpGet("api/comments/card/{cardId}")]
        public async Task<IActionResult> GetByCard(int cardId)
        {
            var comments = await _commentService.GetByCard(cardId);
            return Ok(comments);
        }

        // GET /api/comments/{id}/replies
        [HttpGet("api/comments/{id}/replies")]
        public async Task<IActionResult> GetReplies(int id)
        {
            var replies = await _commentService.GetReplies(id);
            return Ok(replies);
        }

        // GET /api/comments/card/{cardId}/count
        [HttpGet("api/comments/card/{cardId}/count")]
        public async Task<IActionResult> GetCount(int cardId)
        {
            var count = await _commentService.GetCommentCount(cardId);
            return Ok(new { cardId, count });
        }

        // PUT /api/comments/{id}
        [HttpPut("api/comments/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentRequest request)
        {
            var userId  = GetUserId();
            var comment = await _commentService.UpdateComment(id, request, userId);
            return Ok(comment);
        }

        // DELETE /api/comments/{id}
        [HttpDelete("api/comments/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            await _commentService.DeleteComment(id, userId);
            return NoContent();
        }

        // ── Attachment Endpoints ──────────────────────────────────────────────

        // POST /api/attachments
        // multipart/form-data — file AWS S3 pe upload hogi
        // POST /api/attachments
// multipart/form-data — file AWS S3 pe upload hogi
[HttpPost("api/attachments")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> AddAttachment(
    [FromForm] int cardId,
    IFormFile file)  // ← [FromForm] hata do IFormFile se
{
    var uploaderId = GetUserId();
    var request = new AddAttachmentRequest(cardId, uploaderId, file);
    var attachment = await _commentService.AddAttachment(request);
    return Ok(attachment);
}

        // GET /api/attachments/card/{cardId}
        [HttpGet("api/attachments/card/{cardId}")]
        public async Task<IActionResult> GetAttachmentsByCard(int cardId)
        {
            var attachments = await _commentService.GetAttachmentsByCard(cardId);
            return Ok(attachments);
        }

        // DELETE /api/attachments/{id}
        [HttpDelete("api/attachments/{id}")]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var userId = GetUserId();
            await _commentService.DeleteAttachment(id, userId);
            return NoContent();
        }
    }
}