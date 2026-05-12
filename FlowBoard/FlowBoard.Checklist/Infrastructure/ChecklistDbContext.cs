using Microsoft.EntityFrameworkCore;
using FlowBoard.Checklist.Models;

namespace FlowBoard.Checklist.Infrastructure
{
    public class ChecklistDbContext : DbContext
    {
        public ChecklistDbContext(DbContextOptions<ChecklistDbContext> options)
            : base(options) { }

        public DbSet<Label> Labels { get; set; }
        public DbSet<TaskChecklist> Checklists { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cascade delete checklist items when a checklist is deleted
            modelBuilder.Entity<TaskChecklist>()
                .HasMany<ChecklistItem>()
                .WithOne()
                .HasForeignKey(i => i.ChecklistId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index: fast lookup by board
            modelBuilder.Entity<Label>()
                .HasIndex(l => l.BoardId);

            // Index: fast lookup of checklists by card
            modelBuilder.Entity<TaskChecklist>()
                .HasIndex(c => c.CardId);

            // Index: fast lookup of items by checklist
            modelBuilder.Entity<ChecklistItem>()
                .HasIndex(i => i.ChecklistId);
        }
    }
}