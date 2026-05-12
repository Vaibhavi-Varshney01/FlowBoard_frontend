using FlowBoard.Card.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowBoard.Card.Controllers
{
    [ApiController]
    [Route("api/cards")]
    [Authorize]
    public class CardController : ControllerBase
    {
        private readonly ICardService _cardService;

        public CardController(ICardService cardService)
        {
            _cardService = cardService;
        }

        private string GetActorId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        // ── CREATE ────────────────────────────────────────────────────────────

        // POST /api/cards
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCardRequest request)
        {
            try
            {
                var card = await _cardService.CreateCard(request);
                return Ok(card);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── READ ──────────────────────────────────────────────────────────────

        // GET /api/cards/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var card = await _cardService.GetCardById(id);
                return Ok(card);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET /api/cards/list/{listId}
        [HttpGet("list/{listId}")]
        public async Task<IActionResult> GetByList(int listId)
        {
            var cards = await _cardService.GetCardsByList(listId);
            return Ok(cards);
        }

        // GET /api/cards/board/{boardId}
        [HttpGet("board/{boardId}")]
        public async Task<IActionResult> GetByBoard(int boardId)
        {
            var cards = await _cardService.GetCardsByBoard(boardId);
            return Ok(cards);
        }

        // GET /api/cards/assignee/{assigneeId}
        [HttpGet("assignee/{assigneeId}")]
        public async Task<IActionResult> GetByAssignee(string assigneeId)
        {
            var cards = await _cardService.GetCardsByAssignee(assigneeId);
            return Ok(cards);
        }

        // GET /api/cards/overdue
        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdue()
        {
            var cards = await _cardService.GetOverdueCards();
            return Ok(cards);
        }

        // GET /api/cards/{id}/activity
        [HttpGet("{id}/activity")]
        public async Task<IActionResult> GetActivity(int id)
        {
            try
            {
                var activities = await _cardService.GetCardActivity(id);
                return Ok(activities);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET /api/cards/filter
        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] CardFilterRequest filter)
        {
            var cards = await _cardService.FilterCards(filter);
            return Ok(cards);
        }

        // ── UPDATE ────────────────────────────────────────────────────────────

        // PUT /api/cards/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCardRequest request)
        {
            try
            {
                var card = await _cardService.UpdateCard(id, request, GetActorId());
                return Ok(card);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // PUT /api/cards/{id}/move
        [HttpPut("{id}/move")]
        public async Task<IActionResult> Move(int id, [FromBody] MoveCardRequest request)
        {
            try
            {
                var card = await _cardService.MoveCard(id, request, GetActorId());
                return Ok(card);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // PUT /api/cards/list/{listId}/reorder
        [HttpPut("list/{listId}/reorder")]
        public async Task<IActionResult> Reorder(int listId, [FromBody] ReorderCardsRequest request)
        {
            try
            {
                var cards = await _cardService.ReorderCards(listId, request);
                return Ok(cards);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/cards/{id}/assignee
        [HttpPut("{id}/assignee")]
        public async Task<IActionResult> SetAssignee(int id, [FromBody] SetAssigneeRequest request)
        {
            try
            {
                var card = await _cardService.SetAssignee(id, request, GetActorId());
                return Ok(card);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // PUT /api/cards/{id}/priority
        [HttpPut("{id}/priority")]
        public async Task<IActionResult> SetPriority(int id, [FromBody] SetPriorityRequest request)
        {
            try
            {
                var card = await _cardService.SetPriority(id, request, GetActorId());
                return Ok(card);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // ── ARCHIVE ───────────────────────────────────────────────────────────

        // POST /api/cards/{id}/archive
        [HttpPost("{id}/archive")]
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                var card = await _cardService.ArchiveCard(id, GetActorId());
                return Ok(card);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // POST /api/cards/{id}/unarchive
        [HttpPost("{id}/unarchive")]
        public async Task<IActionResult> Unarchive(int id)
        {
            try
            {
                var card = await _cardService.UnarchiveCard(id, GetActorId());
                return Ok(card);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // ── DELETE ────────────────────────────────────────────────────────────

        // DELETE /api/cards/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _cardService.DeleteCard(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // SAGA: DELETE /api/cards/list/{listId}/all
        [HttpDelete("list/{listId}/all")]
        public async Task<IActionResult> DeleteAllByList(int listId)
        {
            await _cardService.DeleteAllCardsByList(listId);
            return NoContent();
        }

        // SAGA: DELETE /api/cards/board/{boardId}/all
        [HttpDelete("board/{boardId}/all")]
        public async Task<IActionResult> DeleteAllByBoard(int boardId)
        {
            await _cardService.DeleteAllCardsByBoard(boardId);
            return NoContent();
        }
    }
}