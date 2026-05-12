using FlowBoard.Workspace.Models;

namespace FlowBoard.Workspace.Repositories
{
    public interface IWorkspaceRepository
    {
        Task<List<Models.Workspace>> FindByOwnerId(string ownerId);
        Task<Models.Workspace?> FindByWorkspaceId(int workspaceId);
        Task<List<Models.Workspace>> FindByMemberUserId(string userId);
        Task<List<Models.Workspace>> FindByVisibility(string visibility);
        Task<bool> ExistsByNameAndOwnerId(string name, string ownerId);
        Task<int> CountByOwnerId(string ownerId);

        Task<Models.Workspace> Save(Models.Workspace workspace);
        Task<Models.Workspace> Update(Models.Workspace workspace);
        Task Delete(int workspaceId);

        // Member operations
        Task<WorkspaceMember?> FindMember(int workspaceId, string userId);
        Task<List<WorkspaceMember>> FindAllMembers(int workspaceId);
        Task<WorkspaceMember> SaveMember(WorkspaceMember member);
        Task<WorkspaceMember> UpdateMember(WorkspaceMember member);
        Task DeleteMember(int workspaceId, string userId);
        Task<bool> IsMember(int workspaceId, string userId);
    }
}