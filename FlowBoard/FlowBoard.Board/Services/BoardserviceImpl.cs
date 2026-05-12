using FlowBoard.Board.Models;
using FlowBoard.Board.Repositories;

namespace FlowBoard.Board.Services
{
    public class BoardServiceImpl : IBoardService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IHttpClientFactory _httpClientFactory; // ADDED

        public BoardServiceImpl(
            IBoardRepository boardRepository,
            IHttpClientFactory httpClientFactory) // ADDED
        {
            _boardRepository    = boardRepository;
            _httpClientFactory  = httpClientFactory; // ADDED
        }

        public async Task<Models.Board> CreateBoard(CreateBoardRequest request)
        {
            var board = new Models.Board
            {
                WorkspaceId = request.WorkspaceId,
                Name        = request.Name,
                Description = request.Description,
                Background  = request.Background,
                Visibility  = request.Visibility.ToUpper(),
                CreatedById = request.CreatedById,
                IsClosed    = false,
                CreatedAt   = DateTime.UtcNow
            };

            var saved = await _boardRepository.Save(board);

            // Auto-add creator as ADMIN board member
            var creatorMember = new BoardMember
            {
                BoardId = saved.BoardId,
                UserId  = request.CreatedById,
                Role    = "ADMIN",
                AddedAt = DateTime.UtcNow
            };
            await _boardRepository.SaveMember(creatorMember);

            return saved;
        }

        public async Task<Models.Board> GetBoardById(int boardId)
            => await _boardRepository.FindByBoardId(boardId)
               ?? throw new KeyNotFoundException($"Board {boardId} not found.");

        public async Task<List<Models.Board>> GetBoardsByWorkspace(int workspaceId)
            => await _boardRepository.FindByWorkspaceId(workspaceId);

        public async Task<List<Models.Board>> GetBoardsByMember(string userId)
            => await _boardRepository.FindByMemberUserId(userId);

        public async Task<Models.Board> UpdateBoard(int boardId, UpdateBoardRequest request, string requestingUserId)
        {
            var board = await _boardRepository.FindByBoardId(boardId)
                ?? throw new KeyNotFoundException($"Board {boardId} not found.");

            if (board.IsClosed)
                throw new InvalidOperationException("Cannot update a closed board.");

            var requester = await _boardRepository.FindMember(boardId, requestingUserId);
            if (board.CreatedById != requestingUserId &&
                (requester == null || requester.Role != "ADMIN"))
                throw new UnauthorizedAccessException("Only the board creator or an ADMIN can update this board.");

            board.Name        = request.Name;
            board.Description = request.Description;
            board.Background  = request.Background;
            board.Visibility  = request.Visibility.ToUpper();

            return await _boardRepository.Update(board);
        }

        public async Task<Models.Board> CloseBoard(int boardId, string requestingUserId)
        {
            var board = await _boardRepository.FindByBoardId(boardId)
                ?? throw new KeyNotFoundException($"Board {boardId} not found.");

            if (board.IsClosed)
                throw new InvalidOperationException("Board is already closed.");

            var requester = await _boardRepository.FindMember(boardId, requestingUserId);
            if (board.CreatedById != requestingUserId &&
                (requester == null || requester.Role != "ADMIN"))
                throw new UnauthorizedAccessException(
                    "Only the board creator or an ADMIN can close this board.");

            board.IsClosed = true;
            return await _boardRepository.Update(board);
        }

        // ── SAGA: Board delete karne se pehle List Service ko call karo ──────
        public async Task DeleteBoard(int boardId, string requestingUserId)
        {
            // Step 1: Board exist karta hai?
            var board = await _boardRepository.FindByBoardId(boardId)
                ?? throw new KeyNotFoundException($"Board {boardId} not found.");

            // Step 2: Sirf creator delete kar sakta hai
            if (board.CreatedById != requestingUserId)
                throw new UnauthorizedAccessException(
                    "Only the board creator can delete this board.");

            try
            {
                // Step 3: SAGA — List Service ko call karo saari lists delete karne ke liye
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.DeleteAsync(
                    $"http://localhost:5004/api/lists/board/{boardId}/all");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(
                        $"[SAGA WARNING] Lists delete nahi hui for board {boardId}. " +
                        $"Status: {response.StatusCode}");
                }
                else
                {
                    Console.WriteLine(
                        $"[SAGA SUCCESS] Board {boardId} ki saari lists delete hui.");
                }

                // Step 4: Board delete karo
                await _boardRepository.Delete(boardId);

                Console.WriteLine(
                    $"[SAGA SUCCESS] Board {boardId} successfully delete hua.");
            }
            catch (HttpRequestException ex)
            {
                // List Service down hai — log karo aur phir bhi board delete karo
                Console.WriteLine(
                    $"[SAGA ERROR] List Service reachable nahi: {ex.Message}");

                // Compensating transaction — board delete karo
                await _boardRepository.Delete(boardId);
            }
        }

        public async Task<BoardMember> AddMember(int boardId, AddMemberRequest request, string requestingUserId)
        {
            var board = await _boardRepository.FindByBoardId(boardId)
                ?? throw new KeyNotFoundException($"Board {boardId} not found.");

            var requester = await _boardRepository.FindMember(boardId, requestingUserId);
            if (board.CreatedById != requestingUserId &&
                (requester == null || requester.Role != "ADMIN"))
                throw new UnauthorizedAccessException("Only the board creator or an ADMIN can add members.");

            if (await _boardRepository.IsMember(boardId, request.UserId))
                throw new InvalidOperationException("User is already a member of this board.");

            var validRoles = new[] { "OBSERVER", "MEMBER", "ADMIN" };
            if (!validRoles.Contains(request.Role.ToUpper()))
                throw new ArgumentException($"Invalid role '{request.Role}'. Must be one of: OBSERVER, MEMBER, ADMIN.");

            var member = new BoardMember
            {
                BoardId = boardId,
                UserId  = request.UserId,
                Role    = request.Role.ToUpper(),
                AddedAt = DateTime.UtcNow
            };

            return await _boardRepository.SaveMember(member);
        }

        public async Task RemoveMember(int boardId, string userId, string requestingUserId)
        {
            var board = await _boardRepository.FindByBoardId(boardId)
                ?? throw new KeyNotFoundException($"Board {boardId} not found.");

            if (board.CreatedById == userId)
                throw new InvalidOperationException("The board creator cannot be removed.");

            var requester = await _boardRepository.FindMember(boardId, requestingUserId);
            if (board.CreatedById != requestingUserId &&
                (requester == null || requester.Role != "ADMIN"))
                throw new UnauthorizedAccessException(
                    "Only the board creator or an ADMIN can remove members.");

            await _boardRepository.DeleteMember(boardId, userId);
        }

        public async Task<BoardMember> UpdateMemberRole(int boardId, string userId, string newRole, string requestingUserId)
        {
            var board = await _boardRepository.FindByBoardId(boardId)
                ?? throw new KeyNotFoundException($"Board {boardId} not found.");

            var requester = await _boardRepository.FindMember(boardId, requestingUserId);
            if (board.CreatedById != requestingUserId &&
                (requester == null || requester.Role != "ADMIN"))
                throw new UnauthorizedAccessException("Only the board creator or an ADMIN can update member roles.");

            var validRoles = new[] { "OBSERVER", "MEMBER", "ADMIN" };
            if (!validRoles.Contains(newRole.ToUpper()))
                throw new ArgumentException($"Invalid role '{newRole}'. Must be one of: OBSERVER, MEMBER, ADMIN.");

            var member = await _boardRepository.FindMember(boardId, userId)
                ?? throw new KeyNotFoundException("Member not found on this board.");

            member.Role = newRole.ToUpper();
            return await _boardRepository.UpdateMember(member);
        }

        public async Task<List<BoardMember>> GetMembers(int boardId)
            => await _boardRepository.FindAllMembers(boardId);
    }
}