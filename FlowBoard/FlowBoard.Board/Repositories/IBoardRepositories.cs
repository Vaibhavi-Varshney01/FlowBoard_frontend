using FlowBoard.Board.Models;

namespace FlowBoard.Board.Repositories
{
    public interface IBoardRepository
    {
        // Board queries
        Task<List<Models.Board>> FindByWorkspaceId(int workspaceId);
        Task<Models.Board?> FindByBoardId(int boardId);
        Task<List<Models.Board>> FindByCreatedById(string userId);
        Task<List<Models.Board>> FindByMemberUserId(string userId);
        Task<List<Models.Board>> FindByVisibility(string visibility);
        Task<int> CountByWorkspaceId(int workspaceId);
        Task<List<Models.Board>> FindByIsClosed(bool isClosed);

        // Board CRUD
        Task<Models.Board> Save(Models.Board board);
        Task<Models.Board> Update(Models.Board board);
        Task Delete(int boardId);

        // Member operations
        Task<BoardMember?> FindMember(int boardId, string userId);
        Task<List<BoardMember>> FindAllMembers(int boardId);
        Task<BoardMember> SaveMember(BoardMember member);
        Task<BoardMember> UpdateMember(BoardMember member);
        Task DeleteMember(int boardId, string userId);
        Task<bool> IsMember(int boardId, string userId);
    }
}