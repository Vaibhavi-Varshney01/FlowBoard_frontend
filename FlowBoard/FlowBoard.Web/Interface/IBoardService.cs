using FlowBoard.Web.DTOs;

namespace FlowBoard.Web.Services
{
    public interface IBoardService
    {
        Task<object?>      GetBoardAsync(int id, string token);
        Task<List<object>> GetByWorkspaceAsync(int workspaceId, string token);
        Task<List<object>> GetByMemberAsync(int userId, string token);
        Task<object?>      CreateBoardAsync(CreateBoardDto dto, string token);
        Task<bool>         UpdateBoardAsync(int id, object model, string token);
        Task<bool>         CloseBoardAsync(int id, string token);
        Task<bool>         DeleteBoardAsync(int id, string token);
        Task<bool>         AddMemberAsync(int boardId, int userId, string role, string token);
        Task<bool>         RemoveMemberAsync(int boardId, int userId, string token);
        Task<List<object>> GetMembersAsync(int boardId, string token);
    }
}