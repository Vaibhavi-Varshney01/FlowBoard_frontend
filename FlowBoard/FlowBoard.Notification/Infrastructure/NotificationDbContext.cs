using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Notification.Infrastructure
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options) { }

        public DbSet<Models.Notification> Notifications => Set<Models.Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Models.Notification>(entity =>
            {
                // Convert enum to string in Postgres for readability
                entity.Property(n => n.Type)
                      .HasConversion<string>();

                // Fast unread-badge query: recipient + is_read composite index
                entity.HasIndex(n => new { n.RecipientId, n.IsRead });

                // Fast deep-link lookup
                entity.HasIndex(n => n.RelatedId);

                // Sort by newest by default (used in most queries)
                entity.HasIndex(n => n.CreatedAt);
            });
        }
    }
}