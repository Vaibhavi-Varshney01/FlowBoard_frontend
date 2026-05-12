using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Board.Infrastructure
{
    public class BoardDbContext : DbContext
    {
        public BoardDbContext(DbContextOptions<BoardDbContext> options)
            : base(options) { }

        public DbSet<Models.Board> Boards => Set<Models.Board>();
        public DbSet<Models.BoardMember> BoardMembers => Set<Models.BoardMember>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraint: one user can only be a member once per board
            modelBuilder.Entity<Models.BoardMember>()
                .HasIndex(bm => new { bm.BoardId, bm.UserId })
                .IsUnique();

            // Cascade delete: removing a board removes all its members
            modelBuilder.Entity<Models.Board>()
                .HasMany(b => b.Members)
                .WithOne(m => m.Board)
                .HasForeignKey(m => m.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}