using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Comment.Infrastructure
{
    public class CommentDbContext : DbContext
    {
        public CommentDbContext(DbContextOptions<CommentDbContext> options)
            : base(options) { }

        public DbSet<Models.Comment>    Comments    => Set<Models.Comment>();
        public DbSet<Models.Attachment> Attachments => Set<Models.Attachment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global query filter — soft deleted comments are invisible by default
            // Use .IgnoreQueryFilters() in repo to access deleted comments
            modelBuilder.Entity<Models.Comment>()
                .HasQueryFilter(c => !c.IsDeleted);

            // Index: fast lookup of comments by card
            modelBuilder.Entity<Models.Comment>()
                .HasIndex(c => c.CardId);

            // Index: fast lookup of replies by parent
            modelBuilder.Entity<Models.Comment>()
                .HasIndex(c => c.ParentCommentId);

            // Index: fast lookup of attachments by card
            modelBuilder.Entity<Models.Attachment>()
                .HasIndex(a => a.CardId);
        }
    }
}