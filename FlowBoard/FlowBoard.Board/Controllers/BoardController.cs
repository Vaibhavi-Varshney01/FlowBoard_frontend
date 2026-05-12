using FlowBoard.Board.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowBoard.Board.Controllers
{
    [ApiController]
    [Route("api/boards")]
    public class BoardController : ControllerBase
    {
        private readonly IBoardService _boardService;

        public BoardController(IBoardService boardService)
        {
            _boardService = boardService;
        }

        /// <summary>Extracts the authenticated user's ID from JWT claims.</summary>
        private string GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("sub");

            if (claim == null)
                throw new UnauthorizedAccessException("User identity claim not found in token.");

            return claim.Value;
        }

        // POST /api/boards
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateBoardRequest request)
        {
            try
            {
                var authenticatedUserId = GetCurrentUserId();
                var safeRequest = request with { CreatedById = authenticatedUserId };
                var board = await _boardService.CreateBoard(safeRequest);
                return Ok(board);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // GET /api/boards/{id}
        // Uses BoardVisibility policy — handler checks JWT claims against board visibility
        [HttpGet("{id}")]
        [Authorize(Policy = "BoardVisibility")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var board = await _boardService.GetBoardById(id);
                return Ok(board);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET /api/boards/workspace/{workspaceId}
        [HttpGet("workspace/{workspaceId}")]
        [Authorize]
        public async Task<IActionResult> GetByWorkspace(int workspaceId)
        {
            var boards = await _boardService.GetBoardsByWorkspace(workspaceId);
            return Ok(boards);
        }

        // GET /api/boards/member/{userId}
        [HttpGet("member/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetByMember(string userId)
        {
            var boards = await _boardService.GetBoardsByMember(userId);
            return Ok(boards);
        }

        // GET /api/boards/my — boards where current user is a member
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyBoards()
        {
            var userId = GetCurrentUserId();
            var boards = await _boardService.GetBoardsByMember(userId);
            return Ok(boards);
        }

        // PUT /api/boards/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBoardRequest request)
        {
            try
            {
                var requestingUserId = GetCurrentUserId();
                var updated = await _boardService.UpdateBoard(id, request, requestingUserId);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // PUT /api/boards/{id}/close
        [HttpPut("{id}/close")]
        [Authorize]
        public async Task<IActionResult> Close(int id)
        {
            try
            {
                var requestingUserId = GetCurrentUserId();
                var board = await _boardService.CloseBoard(id, requestingUserId);
                return Ok(board);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // DELETE /api/boards/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var requestingUserId = GetCurrentUserId();
                await _boardService.DeleteBoard(id, requestingUserId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // POST /api/boards/{id}/members
        [HttpPost("{id}/members")]
        [Authorize]
        public async Task<IActionResult> AddMember(int id, [FromBody] AddMemberRequest request)
        {
            try
            {
                var requestingUserId = GetCurrentUserId();
                var member = await _boardService.AddMember(id, request, requestingUserId);
                return Ok(member);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/boards/{id}/members/{userId}
        [HttpDelete("{id}/members/{userId}")]
        [Authorize]
        public async Task<IActionResult> RemoveMember(int id, string userId)
        {
            try
            {
                var requestingUserId = GetCurrentUserId();
                await _boardService.RemoveMember(id, userId, requestingUserId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // PUT /api/boards/{id}/members/{userId}/role
        [HttpPut("{id}/members/{userId}/role")]
        [Authorize]
        public async Task<IActionResult> UpdateRole(int id, string userId, [FromBody] string newRole)
        {
            try
            {
                var requestingUserId = GetCurrentUserId();
                var member = await _boardService.UpdateMemberRole(id, userId, newRole, requestingUserId);
                return Ok(member);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/boards/{id}/members
        [HttpGet("{id}/members")]
        [Authorize]
        public async Task<IActionResult> GetMembers(int id)
        {
            var members = await _boardService.GetMembers(id);
            return Ok(members);
        }

        [HttpGet("all")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllBoards()
        {
            var userId = GetCurrentUserId();
            var boards = await _boardService.GetBoardsByMember(userId); // Mocking for now, in real app would be GetAll()
            return Ok(boards);
        }
    }
}