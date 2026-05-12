using FlowBoard.List.Infrastructure;
using FlowBoard.List.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.List.Repositories
{
    public class ListRepository : IListRepository
    {
        private readonly ListDbContext _context;

        public ListRepository(ListDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskList>> FindByBoardId(int boardId)
            => await _context.TaskLists
                .Where(l => l.BoardId == boardId)
                .ToListAsync();

        public async Task<TaskList?> FindByListId(int listId)
            => await _context.TaskLists
                .FirstOrDefaultAsync(l => l.ListId == listId);

        public async Task<List<TaskList>> FindByBoardIdOrderByPosition(int boardId)
            => await _context.TaskLists
                .Where(l => l.BoardId == boardId && !l.IsArchived)
                .OrderBy(l => l.Position)
                .ToListAsync();

        public async Task<List<TaskList>> FindByBoardIdAndIsArchived(int boardId, bool isArchived)
            => await _context.TaskLists
                .Where(l => l.BoardId == boardId && l.IsArchived == isArchived)
                .OrderBy(l => l.Position)
                .ToListAsync();

        public async Task<int> CountByBoardId(int boardId)
            => await _context.TaskLists
                .CountAsync(l => l.BoardId == boardId && !l.IsArchived);

        public async Task<int> FindMaxPositionByBoardId(int boardId)
        {
            var hasLists = await _context.TaskLists
                .AnyAsync(l => l.BoardId == boardId && !l.IsArchived);

            if (!hasLists) return 0;

            return await _context.TaskLists
                .Where(l => l.BoardId == boardId && !l.IsArchived)
                .MaxAsync(l => l.Position);
        }

        public async Task DeleteByListId(int listId)
        {
            var list = await _context.TaskLists.FindAsync(listId);
            if (list != null)
            {
                _context.TaskLists.Remove(list);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<TaskList> Save(TaskList list)
        {
            _context.TaskLists.Add(list);
            await _context.SaveChangesAsync();
            return list;
        }

        public async Task<TaskList> Update(TaskList list)
        {
            _context.TaskLists.Update(list);
            await _context.SaveChangesAsync();
            return list;
        }

        /// <summary>
        /// Atomically updates positions for all sibling lists within a transaction scope.
        /// Replaces the Spring Data JPA batch save pattern with EF Core ExecuteUpdateAsync.
        /// </summary>
        public async Task UpdateRangePositions(List<TaskList> lists)
{
    await using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // Step 1: Saari positions ko negative karo (conflict avoid karne ke liye)
        foreach (var list in lists)
        {
            await _context.TaskLists
                .Where(l => l.ListId == list.ListId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(l => l.Position, -list.ListId));
        }

        // Step 2: Ab actual positions daalo
        foreach (var list in lists)
        {
            await _context.TaskLists
                .Where(l => l.ListId == list.ListId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(l => l.Position, list.Position)
                    .SetProperty(l => l.UpdatedAt, DateTime.UtcNow));
        }

        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
    }
}