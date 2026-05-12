using FlowBoard.Workspace.DTOs;
using FlowBoard.Workspace.Models;
using FlowBoard.Workspace.Repositories;

namespace FlowBoard.Workspace.Services
{
    public class WorkspaceServiceImpl : IWorkspaceService
    {
        private readonly IWorkspaceRepository _workspaceRepository;

        public WorkspaceServiceImpl(IWorkspaceRepository workspaceRepository)
        {
            _workspaceRepository = workspaceRepository;
        }

        public async Task<Models.Workspace> CreateWorkspace(CreateWorkspaceRequest request)
        {
            if (await _workspaceRepository.ExistsByNameAndOwnerId(request.Name, request.OwnerId))
                throw new InvalidOperationException(
                    "A workspace with this name already exists for this user.");

            var workspace = new Models.Workspace
            {
                Name        = request.Name,
                Description = request.Description,
                OwnerId     = request.OwnerId,
                Visibility  = request.Visibility.ToUpper(),
                LogoUrl     = request.LogoUrl,
                CreatedAt   = DateTime.UtcNow,
                UpdatedAt   = DateTime.UtcNow
            };

            var saved = await _workspaceRepository.Save(workspace);

            // Auto-add owner as ADMIN member
            var ownerMember = new WorkspaceMember
            {
                WorkspaceId = saved.WorkspaceId,
                UserId      = request.OwnerId,
                Role        = "ADMIN",
                JoinedAt    = DateTime.UtcNow
            };
            await _workspaceRepository.SaveMember(ownerMember);

            return saved;
        }

        public async Task<Models.Workspace> GetById(int workspaceId)
            => await _workspaceRepository.FindByWorkspaceId(workspaceId)
               ?? throw new KeyNotFoundException($"Workspace {workspaceId} not found.");

        public async Task<List<Models.Workspace>> GetByOwner(string ownerId)
            => await _workspaceRepository.FindByOwnerId(ownerId);

        public async Task<List<Models.Workspace>> GetByMember(string userId)
            => await _workspaceRepository.FindByMemberUserId(userId);

        public async Task<List<Models.Workspace>> GetPublicWorkspaces()
            => await _workspaceRepository.FindByVisibility("PUBLIC");

        public async Task<Models.Workspace> UpdateWorkspace(
            int workspaceId,
            UpdateWorkspaceRequest request)
        {
            var workspace = await _workspaceRepository.FindByWorkspaceId(workspaceId)
                ?? throw new KeyNotFoundException($"Workspace {workspaceId} not found.");

            workspace.Name        = request.Name;
            workspace.Description = request.Description;
            workspace.Visibility  = request.Visibility.ToUpper();
            workspace.LogoUrl     = request.LogoUrl;

            return await _workspaceRepository.Update(workspace);
        }

        public async Task DeleteWorkspace(int workspaceId, string requestingUserId)
        {
            var workspace = await _workspaceRepository.FindByWorkspaceId(workspaceId)
                ?? throw new KeyNotFoundException($"Workspace {workspaceId} not found.");

            if (workspace.OwnerId != requestingUserId)
                throw new UnauthorizedAccessException(
                    "Only the owner can delete this workspace.");

            await _workspaceRepository.Delete(workspaceId);
        }

        public async Task<WorkspaceMember> AddMember(
            int workspaceId,
            AddMemberRequest request)
        {
            _ = await _workspaceRepository.FindByWorkspaceId(workspaceId)
                ?? throw new KeyNotFoundException($"Workspace {workspaceId} not found.");

            if (await _workspaceRepository.IsMember(workspaceId, request.UserId))
                throw new InvalidOperationException("User is already a member.");

            var member = new WorkspaceMember
            {
                WorkspaceId = workspaceId,
                UserId      = request.UserId,
                Role        = request.Role.ToUpper(),
                JoinedAt    = DateTime.UtcNow
            };

            return await _workspaceRepository.SaveMember(member);
        }

        public async Task RemoveMember(
            int workspaceId,
            string userId,
            string requestingUserId)
        {
            var workspace = await _workspaceRepository.FindByWorkspaceId(workspaceId)
                ?? throw new KeyNotFoundException($"Workspace {workspaceId} not found.");

            if (workspace.OwnerId == userId)
                throw new InvalidOperationException(
                    "The workspace owner cannot be removed.");

            var requester = await _workspaceRepository.FindMember(workspaceId, requestingUserId);
            if (requester == null || (requester.Role != "ADMIN" && requestingUserId != workspace.OwnerId))
                throw new UnauthorizedAccessException(
                    "Only an admin can remove members.");

            await _workspaceRepository.DeleteMember(workspaceId, userId);
        }

        public async Task<WorkspaceMember> UpdateMemberRole(
            int workspaceId,
            string userId,
            string newRole)
        {
            var member = await _workspaceRepository.FindMember(workspaceId, userId)
                ?? throw new KeyNotFoundException("Member not found in this workspace.");

            member.Role = newRole.ToUpper();
            return await _workspaceRepository.UpdateMember(member);
        }

        public async Task<List<WorkspaceMember>> GetMembers(int workspaceId)
            => await _workspaceRepository.FindAllMembers(workspaceId);
    }
}