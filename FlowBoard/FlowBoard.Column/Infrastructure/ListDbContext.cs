using FlowBoard.List.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.List.Infrastructure
{
    public class ListDbContext : DbContext
    {
        public ListDbContext(DbContextOptions<ListDbContext> options)
            : base(options) { }

        public DbSet<TaskList> TaskLists => Set<TaskList>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique position per board
            modelBuilder.Entity<TaskList>()
                .HasIndex(l => new { l.BoardId, l.Position })
                .IsUnique();

            // Index for faster lookups by board
            modelBuilder.Entity<TaskList>()
                .HasIndex(l => l.BoardId);
        }
    }
}
