using FlowBoard.Web.DTOs;

namespace FlowBoard.Web.Services
{
    public interface IWorkspaceService
    {
        Task<object?>       GetWorkspaceAsync(int id, string token);
        Task<List<object>>  GetByOwnerAsync(int ownerId, string token);
        Task<List<object>>  GetByMemberAsync(int userId, string token);
        Task<object?>       CreateWorkspaceAsync(CreateWorkspaceDto dto, string token);
        Task<bool>          UpdateWorkspaceAsync(int id, object model, string token);
        Task<bool>          DeleteWorkspaceAsync(int id, string token);
        Task<bool>          AddMemberAsync(int workspaceId, int userId, string role, string token);
        Task<bool>          RemoveMemberAsync(int workspaceId, int userId, string token);
        Task<List<object>>  GetMembersAsync(int workspaceId, string token);
    }
}