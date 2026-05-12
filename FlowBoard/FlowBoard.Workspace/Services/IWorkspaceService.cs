using FlowBoard.Workspace.DTOs;
using FlowBoard.Workspace.Models;

namespace FlowBoard.Workspace.Services
{
    public interface IWorkspaceService
    {
        Task<Models.Workspace> CreateWorkspace(CreateWorkspaceRequest request);
        Task<Models.Workspace> GetById(int workspaceId);
        Task<List<Models.Workspace>> GetByOwner(string ownerId);
        Task<List<Models.Workspace>> GetByMember(string userId);
        Task<List<Models.Workspace>> GetPublicWorkspaces();
        Task<Models.Workspace> UpdateWorkspace(int workspaceId, UpdateWorkspaceRequest request);
        Task DeleteWorkspace(int workspaceId, string requestingUserId);
        Task<WorkspaceMember> AddMember(int workspaceId, AddMemberRequest request);
        Task RemoveMember(int workspaceId, string userId, string requestingUserId);
        Task<WorkspaceMember> UpdateMemberRole(int workspaceId, string userId, string newRole);
        Task<List<WorkspaceMember>> GetMembers(int workspaceId);
    }
}