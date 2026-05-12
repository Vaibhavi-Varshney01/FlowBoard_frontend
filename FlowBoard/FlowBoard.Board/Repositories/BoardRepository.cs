using FlowBoard.Board.Infrastructure;
using FlowBoard.Board.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Board.Repositories
{
    public class BoardRepository : IBoardRepository
    {
        private readonly BoardDbContext _context;

        public BoardRepository(BoardDbContext context)
        {
            _context = context;
        }

        public async Task<List<Models.Board>> FindByWorkspaceId(int workspaceId)
            => await _context.Boards
                .Where(b => b.WorkspaceId == workspaceId)
                .Include(b => b.Members)
                .ToListAsync();

        public async Task<Models.Board?> FindByBoardId(int boardId)
            => await _context.Boards
                .Include(b => b.Members)
                .FirstOrDefaultAsync(b => b.BoardId == boardId);

        public async Task<List<Models.Board>> FindByCreatedById(string userId)
            => await _context.Boards
                .Where(b => b.CreatedById == userId)
                .Include(b => b.Members)
                .ToListAsync();

        public async Task<List<Models.Board>> FindByMemberUserId(string userId)
            => await _context.Boards
                .Include(b => b.Members)
                .Where(b => b.Members.Any(m => m.UserId == userId))
                .ToListAsync();

        public async Task<List<Models.Board>> FindByVisibility(string visibility)
            => await _context.Boards
                .Where(b => b.Visibility == visibility)
                .ToListAsync();

        public async Task<int> CountByWorkspaceId(int workspaceId)
            => await _context.Boards
                .CountAsync(b => b.WorkspaceId == workspaceId);

        public async Task<List<Models.Board>> FindByIsClosed(bool isClosed)
            => await _context.Boards
                .Where(b => b.IsClosed == isClosed)
                .Include(b => b.Members)
                .ToListAsync();

        public async Task<Models.Board> Save(Models.Board board)
        {
            _context.Boards.Add(board);
            await _context.SaveChangesAsync();
            return board;
        }

        public async Task<Models.Board> Update(Models.Board board)
        {
            _context.Boards.Update(board);
            await _context.SaveChangesAsync();
            return board;
        }

        public async Task Delete(int boardId)
        {
            var board = await _context.Boards.FindAsync(boardId);
            if (board != null)
            {
                _context.Boards.Remove(board);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<BoardMember?> FindMember(int boardId, string userId)
            => await _context.BoardMembers
                .FirstOrDefaultAsync(m => m.BoardId == boardId && m.UserId == userId);

        public async Task<List<BoardMember>> FindAllMembers(int boardId)
            => await _context.BoardMembers
                .Where(m => m.BoardId == boardId)
                .ToListAsync();

        public async Task<BoardMember> SaveMember(BoardMember member)
        {
            _context.BoardMembers.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<BoardMember> UpdateMember(BoardMember member)
        {
            _context.BoardMembers.Update(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task DeleteMember(int boardId, string userId)
        {
            var member = await FindMember(boardId, userId);
            if (member != null)
            {
                _context.BoardMembers.Remove(member);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsMember(int boardId, string userId)
            => await _context.BoardMembers
                .AnyAsync(m => m.BoardId == boardId && m.UserId == userId);
    }
}