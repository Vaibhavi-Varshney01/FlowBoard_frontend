using FlowBoard.Workspace.Data;
using FlowBoard.Workspace.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Workspace.Repositories
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private readonly WorkspaceDbContext _context;

        public WorkspaceRepository(WorkspaceDbContext context)
        {
            _context = context;
        }

        public async Task<List<Models.Workspace>> FindByOwnerId(string ownerId)
            => await _context.Workspaces
                .Where(w => w.OwnerId == ownerId)
                .Include(w => w.Members)
                .ToListAsync();

        public async Task<Models.Workspace?> FindByWorkspaceId(int workspaceId)
            => await _context.Workspaces
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.WorkspaceId == workspaceId);

        public async Task<List<Models.Workspace>> FindByMemberUserId(string userId)
            => await _context.Workspaces
                .Include(w => w.Members)
                .Where(w => w.Members.Any(m => m.UserId == userId))
                .ToListAsync();

        public async Task<List<Models.Workspace>> FindByVisibility(string visibility)
            => await _context.Workspaces
                .Where(w => w.Visibility == visibility)
                .ToListAsync();

        public async Task<bool> ExistsByNameAndOwnerId(string name, string ownerId)
            => await _context.Workspaces
                .AnyAsync(w => w.Name == name && w.OwnerId == ownerId);

        public async Task<int> CountByOwnerId(string ownerId)
            => await _context.Workspaces
                .CountAsync(w => w.OwnerId == ownerId);

        public async Task<Models.Workspace> Save(Models.Workspace workspace)
        {
            _context.Workspaces.Add(workspace);
            await _context.SaveChangesAsync();
            return workspace;
        }

        public async Task<Models.Workspace> Update(Models.Workspace workspace)
        {
            workspace.UpdatedAt = DateTime.UtcNow;
            _context.Workspaces.Update(workspace);
            await _context.SaveChangesAsync();
            return workspace;
        }

        public async Task Delete(int workspaceId)
        {
            var workspace = await _context.Workspaces.FindAsync(workspaceId);
            if (workspace != null)
            {
                _context.Workspaces.Remove(workspace);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<WorkspaceMember?> FindMember(int workspaceId, string userId)
            => await _context.WorkspaceMembers
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId
                                       && m.UserId == userId);

        public async Task<List<WorkspaceMember>> FindAllMembers(int workspaceId)
            => await _context.WorkspaceMembers
                .Where(m => m.WorkspaceId == workspaceId)
                .ToListAsync();

        public async Task<WorkspaceMember> SaveMember(WorkspaceMember member)
        {
            _context.WorkspaceMembers.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<WorkspaceMember> UpdateMember(WorkspaceMember member)
        {
            _context.WorkspaceMembers.Update(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task DeleteMember(int workspaceId, string userId)
        {
            var member = await FindMember(workspaceId, userId);
            if (member != null)
            {
                _context.WorkspaceMembers.Remove(member);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsMember(int workspaceId, string userId)
            => await _context.WorkspaceMembers
                .AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId);
    }
}