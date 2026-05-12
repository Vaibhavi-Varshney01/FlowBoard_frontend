using FlowBoard.Workspace.Models;
using Microsoft.EntityFrameworkCore;

// ✅ Namespace aligned with folder name (Data)
namespace FlowBoard.Workspace.Data
{
    public class WorkspaceDbContext : DbContext
    {
        public WorkspaceDbContext(DbContextOptions<WorkspaceDbContext> options)
            : base(options) { }

        public DbSet<Models.Workspace> Workspaces => Set<Models.Workspace>();
        public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraint: one user can only be a member once per workspace
            modelBuilder.Entity<WorkspaceMember>()
                .HasIndex(wm => new { wm.WorkspaceId, wm.UserId })
                .IsUnique();

            // Cascade delete: removing a workspace removes all its members
            modelBuilder.Entity<Models.Workspace>()
                .HasMany(w => w.Members)
                .WithOne(m => m.Workspace)
                .HasForeignKey(m => m.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}