using FlowBoard.Board.Models;

namespace FlowBoard.Board.Services
{
    public record CreateBoardRequest(
        int WorkspaceId,
        string Name,
        string? Description,
        string? Background,
        string Visibility,
        string CreatedById
    );

    public record UpdateBoardRequest(
        string Name,
        string? Description,
        string? Background,
        string Visibility
    );

    public record AddMemberRequest(
        string UserId,
        string Role
    );

    public interface IBoardService
    {
        // Board CRUD
        Task<Models.Board> CreateBoard(CreateBoardRequest request);
        Task<Models.Board> GetBoardById(int boardId);
        Task<List<Models.Board>> GetBoardsByWorkspace(int workspaceId);
        Task<List<Models.Board>> GetBoardsByMember(string userId);
        Task<Models.Board> UpdateBoard(int boardId, UpdateBoardRequest request, string requestingUserId);
        Task<Models.Board> CloseBoard(int boardId, string requestingUserId);
        Task DeleteBoard(int boardId, string requestingUserId);

        // Member management
        Task<BoardMember> AddMember(int boardId, AddMemberRequest request, string requestingUserId);
        Task RemoveMember(int boardId, string userId, string requestingUserId);
        Task<BoardMember> UpdateMemberRole(int boardId, string userId, string newRole, string requestingUserId);
        Task<List<BoardMember>> GetMembers(int boardId);
    }
}