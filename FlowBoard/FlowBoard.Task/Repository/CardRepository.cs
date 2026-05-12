using FlowBoard.Card.Infrastructure;
using FlowBoard.Card.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Card.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly CardDbContext _context;

        public CardRepository(CardDbContext context)
        {
            _context = context;
        }

        public async Task<List<Models.Card>> FindByListId(int listId)
            => await _context.Cards
                .Where(c => c.ListId == listId)
                .ToListAsync();

        public async Task<List<Models.Card>> FindByBoardId(int boardId)
            => await _context.Cards
                .Where(c => c.BoardId == boardId && !c.IsArchived)
                .OrderBy(c => c.ListId)
                .ThenBy(c => c.Position)
                .ToListAsync();

        public async Task<List<Models.Card>> FindByAssigneeId(string assigneeId)
            => await _context.Cards
                .Where(c => c.AssigneeId == assigneeId && !c.IsArchived)
                .OrderBy(c => c.DueDate)
                .ToListAsync();

        public async Task<Models.Card?> FindByCardId(int cardId)
            => await _context.Cards.FirstOrDefaultAsync(c => c.CardId == cardId);

        public async Task<List<Models.Card>> FindByListIdOrderByPosition(int listId)
            => await _context.Cards
                .Where(c => c.ListId == listId && !c.IsArchived)
                .OrderBy(c => c.Position)
                .ToListAsync();

        public async Task<List<Models.Card>> FindByDueDateBefore(DateTime cutoff)
            => await _context.Cards
                .Where(c => c.DueDate.HasValue
                         && c.DueDate.Value < cutoff
                         && !c.IsArchived
                         && c.Status != CardStatus.DONE)
                .ToListAsync();

        public async Task<List<Models.Card>> FindByPriority(Priority priority)
            => await _context.Cards
                .Where(c => c.Priority == priority && !c.IsArchived)
                .OrderBy(c => c.DueDate)
                .ToListAsync();

        public async Task<List<Models.Card>> FindByStatus(CardStatus status)
            => await _context.Cards
                .Where(c => c.Status == status && !c.IsArchived)
                .OrderBy(c => c.DueDate)
                .ToListAsync();

        public async Task<int> CountByListId(int listId)
            => await _context.Cards.CountAsync(c => c.ListId == listId && !c.IsArchived);

        public async Task<Models.Card> Save(Models.Card card)
        {
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task<Models.Card> Update(Models.Card card)
        {
            _context.Cards.Update(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task DeleteByCardId(int cardId)
        {
            var card = await _context.Cards.FindAsync(cardId);
            if (card != null)
            {
                _context.Cards.Remove(card);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Atomically updates positions for all sibling cards within a transaction.
        /// Uses two-pass negative offset pattern to avoid unique constraint conflicts.
        /// </summary>
        public async Task UpdateRangePositions(List<Models.Card> cards)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Pass 1: set temporary negative positions to avoid constraint conflicts
                foreach (var card in cards)
                {
                    await _context.Cards
                        .Where(c => c.CardId == card.CardId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(c => c.Position, -card.CardId));
                }

                // Pass 2: set actual target positions
                foreach (var card in cards)
                {
                    await _context.Cards
                        .Where(c => c.CardId == card.CardId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(c => c.Position, card.Position)
                            .SetProperty(c => c.UpdatedAt, DateTime.UtcNow));
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ── Activity Feed ─────────────────────────────────────────────────────

        public async Task LogActivity(CardActivity activity)
        {
            _context.CardActivities.Add(activity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CardActivity>> GetActivitiesByCardId(int cardId)
            => await _context.CardActivities
                .Where(a => a.CardId == cardId)
                .OrderByDescending(a => a.OccurredAt)
                .ToListAsync();
    }
}