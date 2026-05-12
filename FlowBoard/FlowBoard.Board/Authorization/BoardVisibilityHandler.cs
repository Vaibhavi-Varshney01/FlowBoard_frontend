using FlowBoard.Board.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FlowBoard.Board.Authorization
{
    /// <summary>
    /// Checks JWT claims to enforce board visibility policy:
    /// - PUBLIC boards: accessible by anyone (authenticated or not)
    /// - PRIVATE boards: only accessible by authenticated board members
    /// </summary>
    public class BoardVisibilityHandler : AuthorizationHandler<BoardVisibilityRequirement>
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BoardVisibilityHandler(
            IBoardRepository boardRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _boardRepository = boardRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            BoardVisibilityRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                context.Fail();
                return;
            }

            // Extract boardId from route
            if (!httpContext.Request.RouteValues.TryGetValue("id", out var idValue)
                || !int.TryParse(idValue?.ToString(), out var boardId))
            {
                context.Fail();
                return;
            }

            var board = await _boardRepository.FindByBoardId(boardId);
            if (board == null)
            {
                // Board not found — let the controller return 404
                context.Succeed(requirement);
                return;
            }

            // PUBLIC boards: always allow
            if (board.Visibility == "PUBLIC")
            {
                context.Succeed(requirement);
                return;
            }

            // PRIVATE boards: user must be authenticated and a board member
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                context.Fail();
                return;
            }

            var isMember = await _boardRepository.IsMember(boardId, userId);
            if (isMember || board.CreatedById == userId)
                context.Succeed(requirement);
            else
                context.Fail();
        }
    }
}