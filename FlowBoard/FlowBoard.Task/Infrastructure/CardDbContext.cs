using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Card.Infrastructure
{
    public class CardDbContext : DbContext
    {
        public CardDbContext(DbContextOptions<CardDbContext> options)
            : base(options) { }

        public DbSet<Models.Card> Cards           => Set<Models.Card>();
        public DbSet<Models.CardActivity> CardActivities => Set<Models.CardActivity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Store enums as strings for readability in the database
            modelBuilder.Entity<Models.Card>()
                .Property(c => c.Priority)
                .HasConversion<string>();

            modelBuilder.Entity<Models.Card>()
                .Property(c => c.Status)
                .HasConversion<string>();

            // Unique position per list among active (non-archived) cards
            modelBuilder.Entity<Models.Card>()
                .HasIndex(c => new { c.ListId, c.Position })
                .HasFilter("is_archived = false")
                .IsUnique();

            // Activity feed: card FK, no cascade delete (immutable audit trail)
            modelBuilder.Entity<Models.CardActivity>()
                .HasOne(a => a.Card)
                .WithMany()
                .HasForeignKey(a => a.CardId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fast lookups
            modelBuilder.Entity<Models.Card>()
                .HasIndex(c => c.BoardId);

            modelBuilder.Entity<Models.Card>()
                .HasIndex(c => c.AssigneeId);

            modelBuilder.Entity<Models.Card>()
                .HasIndex(c => c.DueDate);

            modelBuilder.Entity<Models.CardActivity>()
                .HasIndex(a => a.CardId);
        }
    }
}