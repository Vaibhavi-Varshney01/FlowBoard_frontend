using FlowBoard.Card.Models;
using FlowBoard.Card.Repositories;

namespace FlowBoard.Card.Services
{
    public class CardServiceImpl : ICardService
    {
        private readonly ICardRepository _cardRepository;

        public CardServiceImpl(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
        }

        // ── CRUD ──────────────────────────────────────────────────────────────

        public async Task<Models.Card> CreateCard(CreateCardRequest request)
        {
            var siblings = await _cardRepository.FindByListIdOrderByPosition(request.ListId);
            var maxPosition = siblings.Any() ? siblings.Max(c => c.Position) : 0;

            var card = new Models.Card
            {
                ListId      = request.ListId,
                BoardId     = request.BoardId,
                Title       = request.Title,
                Description = request.Description,
                Priority    = request.Priority,
                Status      = request.Status,
                DueDate     = request.DueDate,
                StartDate   = request.StartDate,
                AssigneeId  = request.AssigneeId,
                CreatedById = request.CreatedById,
                CoverColor  = request.CoverColor,
                Position    = maxPosition + 1,
                IsArchived  = false,
                CreatedAt   = DateTime.UtcNow,
                UpdatedAt   = DateTime.UtcNow
            };

            var saved = await _cardRepository.Save(card);

            await _cardRepository.LogActivity(new CardActivity
            {
                CardId     = saved.CardId,
                ActorId    = request.CreatedById,
                Action     = "CREATED",
                Detail     = $"Card '{saved.Title}' created in list {saved.ListId}.",
                OccurredAt = DateTime.UtcNow
            });

            return saved;
        }

        public async Task<Models.Card> GetCardById(int cardId)
        {
            var card = await _cardRepository.FindByCardId(cardId);
            if (card == null)
                throw new KeyNotFoundException($"Card {cardId} not found.");
            return card;
        }

        public async Task<List<Models.Card>> GetCardsByList(int listId)
            => await _cardRepository.FindByListIdOrderByPosition(listId);

        public async Task<List<Models.Card>> GetCardsByBoard(int boardId)
            => await _cardRepository.FindByBoardId(boardId);

        public async Task<List<Models.Card>> GetCardsByAssignee(string assigneeId)
            => await _cardRepository.FindByAssigneeId(assigneeId);

        public async Task<Models.Card> UpdateCard(int cardId, UpdateCardRequest request, string actorId)
        {
            var card = await _cardRepository.FindByCardId(cardId);
            if (card == null)
                throw new KeyNotFoundException($"Card {cardId} not found.");

            if (card.IsArchived)
                throw new InvalidOperationException($"Card {cardId} is archived and cannot be updated.");

            var changes = new List<string>();

            if (card.Title != request.Title)
                changes.Add($"Title: '{card.Title}' → '{request.Title}'");
            if (card.Priority != request.Priority)
                changes.Add($"Priority: {card.Priority} → {request.Priority}");
            if (card.Status != request.Status)
                changes.Add($"Status: {card.Status} → {request.Status}");
            if (card.DueDate != request.DueDate)
                changes.Add($"DueDate changed");

            card.Title       = request.Title;
            card.Description = request.Description;
            card.Priority    = request.Priority;
            card.Status      = request.Status;
            card.DueDate     = request.DueDate;
            card.StartDate   = request.StartDate;
            card.CoverColor  = request.CoverColor;
            card.UpdatedAt   = DateTime.UtcNow;

            var updated = await _cardRepository.Update(card);

            if (changes.Any())
            {
                await _cardRepository.LogActivity(new CardActivity
                {
                    CardId     = cardId,
                    ActorId    = actorId,
                    Action     = "UPDATED",
                    Detail     = string.Join("; ", changes),
                    OccurredAt = DateTime.UtcNow
                });
            }

            return updated;
        }

        public async Task DeleteCard(int cardId)
        {
            var card = await _cardRepository.FindByCardId(cardId);
            if (card == null)
                throw new KeyNotFoundException($"Card {cardId} not found.");

            var siblings = await _cardRepository.FindByListIdOrderByPosition(card.ListId);
            await _cardRepository.DeleteByCardId(cardId);

            var remaining = siblings
                .Where(c => c.CardId != cardId)
                .OrderBy(c => c.Position)
                .ToList();

            for (int i = 0; i < remaining.Count; i++)
                remaining[i].Position = i + 1;

            if (remaining.Any())
                await _cardRepository.UpdateRangePositions(remaining);
        }

        // ── Move & Reorder ────────────────────────────────────────────────────

        public async Task<Models.Card> MoveCard(int cardId, MoveCardRequest request, string actorId)
        {
            var card = await _cardRepository.FindByCardId(cardId);
            if (card == null)
                throw new KeyNotFoundException($"Card {cardId} not found.");

            if (card.IsArchived)
                throw new InvalidOperationException($"Card {cardId} is archived and cannot be moved.");

            var originListId = card.ListId;

            // Compact origin list after removal
            var originSiblings = await _cardRepository.FindByListIdOrderByPosition(originListId);
            var originRemaining = originSiblings
                .Where(c => c.CardId != cardId)
                .OrderBy(c => c.Position)
                .ToList();
            for (int i = 0; i < originRemaining.Count; i++)
                originRemaining[i].Position = i + 1;

            // Make room in target list at requested position
            var targetSiblings = await _cardRepository.FindByListIdOrderByPosition(request.TargetListId);
            var clampedPosition = Math.Clamp(request.TargetPosition, 1, targetSiblings.Count + 1);

            foreach (var sibling in targetSiblings.Where(c => c.Position >= clampedPosition))
                sibling.Position++;

            card.ListId    = request.TargetListId;
            card.BoardId   = request.TargetBoardId;
            card.Position  = clampedPosition;
            card.UpdatedAt = DateTime.UtcNow;

            var moved = await _cardRepository.Update(card);

            if (originRemaining.Any())
                await _cardRepository.UpdateRangePositions(originRemaining);

            if (targetSiblings.Any())
                await _cardRepository.UpdateRangePositions(targetSiblings);

            await _cardRepository.LogActivity(new CardActivity
            {
                CardId     = cardId,
                ActorId    = actorId,
                Action     = "MOVED",
                Detail     = $"Moved from list {originListId} to list {request.TargetListId} at position {clampedPosition}.",
                OccurredAt = DateTime.UtcNow
            });

            return moved;
        }

        public async Task<List<Models.Card>> ReorderCards(int listId, ReorderCardsRequest request)
        {
            var activeCards = await _cardRepository.FindByListIdOrderByPosition(listId);

            if (request.Positions.Count != activeCards.Count)
                throw new ArgumentException(
                    $"Payload has {request.Positions.Count} entries but list has {activeCards.Count} active cards.");

            var cardMap = activeCards.ToDictionary(c => c.CardId);

            foreach (var entry in request.Positions)
            {
                if (!cardMap.ContainsKey(entry.CardId))
                    throw new ArgumentException(
                        $"Card {entry.CardId} does not belong to list {listId} or is archived.");
            }

            foreach (var entry in request.Positions)
                cardMap[entry.CardId].Position = entry.Position;

            await _cardRepository.UpdateRangePositions(activeCards);

            return await _cardRepository.FindByListIdOrderByPosition(listId);
        }

        // ── Archival ──────────────────────────────────────────────────────────

        public async Task<Models.Card> ArchiveCard(int cardId, string actorId)
        {
            var card = await _cardRepository.FindByCardId(cardId);
            if (card == null)
                throw new KeyNotFoundException($"Card {cardId} not found.");

            if (card.IsArchived)
                throw new InvalidOperationException($"Card {cardId} is already archived.");

            card.IsArchived = true;
            card.UpdatedAt  = DateTime.UtcNow;

            var updated = await _cardRepository.Update(card);
            await CompactPositions(card.ListId);

            await _cardRepository.LogActivity(new CardActivity
            {
                CardId     = cardId,
                ActorId    = actorId,
                Action     = "ARCHIVED",
                Detail     = $"Card '{card.Title}' archived.",
                OccurredAt = DateTime.UtcNow
            });

            return updated;
        }

        public async Task<Models.Card> UnarchiveCard(int cardId, string actorId)
        {
            var card = await _cardRepository.FindByCardId(cardId);
            if (card == null)
                throw new KeyNotFoundException($"Card {cardId} not found.");

            if (!card.IsArchived)
                throw new InvalidOperationException($"Card {cardId} is not archived.");

            var siblings = await _cardRepository.FindByListIdOrderByPosition(card.ListId);
            var maxPosition = siblings.Any() ? siblings.Max(c => c.Position) : 0;

            card.IsArchived = false;
            card.Position   = maxPosition + 1;
            card.UpdatedAt  = DateTime.UtcNow;

            var updated = await _cardRepository.Update(card);

            await _cardRepository.LogActivity(new CardActivity
            {
                CardId     = cardId,
                ActorId    = actorId,
                Action     = "UNARCHIVED",
                Detail     = $"Card '{card.Title}' unarchived to position {card.Position}.",
                OccurredAt = DateTime.UtcNow
            });

            return updated;
        }

        // ── Assignment & Priority ─────────────────────────────────────────────

        public async Task<Models.Card> SetAssignee(int cardId, SetAssigneeRequest request, string actorId)
        {
            var card = await _cardRepository.FindByCardId(cardId);
            if (card == null)
                throw new KeyNotFoundException($"Card {cardId} not found.");

            if (card.IsArchived)
                throw new InvalidOperationException($"Card {cardId} is archived.");

            var previousAssignee = card.AssigneeId;
            card.AssigneeId = request.AssigneeId;
            card.UpdatedAt  = DateTime.UtcNow;

            var updated = await _cardRepository.Update(card);

            string detail = !string.IsNullOrEmpty(request.AssigneeId)
                ? $"Assigned to user {request.AssigneeId} (was {previousAssignee ?? "unassigned"})."
                : $"Unassigned (was user {previousAssignee}).";

            await _cardRepository.LogActivity(new CardActivity
            {
                CardId     = cardId,
                ActorId    = actorId,
                Action     = "ASSIGNEE_CHANGED",
                Detail     = detail,
                OccurredAt = DateTime.UtcNow
            });

            return updated;
        }

        public async Task<Models.Card> SetPriority(int cardId, SetPriorityRequest request, string actorId)
        {
            var card = await _cardRepository.FindByCardId(cardId);
            if (card == null)
                throw new KeyNotFoundException($"Card {cardId} not found.");

            if (card.IsArchived)
                throw new InvalidOperationException($"Card {cardId} is archived.");

            var previousPriority = card.Priority;
            card.Priority  = request.Priority;
            card.UpdatedAt = DateTime.UtcNow;

            var updated = await _cardRepository.Update(card);

            await _cardRepository.LogActivity(new CardActivity
            {
                CardId     = cardId,
                ActorId    = actorId,
                Action     = "PRIORITY_CHANGED",
                Detail     = $"Priority changed: {previousPriority} to {request.Priority}.",
                OccurredAt = DateTime.UtcNow
            });

            return updated;
        }

        // ── Overdue detection ─────────────────────────────────────────────────

        public async Task<List<Models.Card>> GetOverdueCards()
            => await _cardRepository.FindByDueDateBefore(DateTime.UtcNow);

        // ── Activity feed ─────────────────────────────────────────────────────

        public async Task<List<CardActivity>> GetCardActivity(int cardId)
        {
            var card = await _cardRepository.FindByCardId(cardId);
            if (card == null)
                throw new KeyNotFoundException($"Card {cardId} not found.");

            return await _cardRepository.GetActivitiesByCardId(cardId);
        }

        // ── Multi-criteria filter ─────────────────────────────────────────────

        public async Task<List<Models.Card>> FilterCards(CardFilterRequest filter)
        {
            IEnumerable<Models.Card> results;

            if (!string.IsNullOrEmpty(filter.AssigneeId))
                results = await _cardRepository.FindByAssigneeId(filter.AssigneeId);
            else if (filter.Status.HasValue)
                results = await _cardRepository.FindByStatus(filter.Status.Value);
            else if (filter.Priority.HasValue)
                results = await _cardRepository.FindByPriority(filter.Priority.Value);
            else if (filter.DueDateBefore.HasValue)
                results = await _cardRepository.FindByDueDateBefore(filter.DueDateBefore.Value);
            else
                return new List<Models.Card>();

            if (!string.IsNullOrEmpty(filter.AssigneeId))
                results = results.Where(c => c.AssigneeId == filter.AssigneeId);
            if (filter.Status.HasValue)
                results = results.Where(c => c.Status == filter.Status.Value);
            if (filter.Priority.HasValue)
                results = results.Where(c => c.Priority == filter.Priority.Value);
            if (filter.DueDateBefore.HasValue)
                results = results.Where(c => c.DueDate.HasValue && c.DueDate.Value < filter.DueDateBefore.Value);

            return results.ToList();
        }

        // ── SAGA ──────────────────────────────────────────────────────────────

        public async Task DeleteAllCardsByList(int listId)
        {
            var cards = await _cardRepository.FindByListId(listId);
            foreach (var card in cards)
                await _cardRepository.DeleteByCardId(card.CardId);

            Console.WriteLine($"[SAGA] List {listId} ki {cards.Count} cards delete hui.");
        }

        public async Task DeleteAllCardsByBoard(int boardId)
        {
            var cards = await _cardRepository.FindByBoardId(boardId);
            foreach (var card in cards)
                await _cardRepository.DeleteByCardId(card.CardId);

            Console.WriteLine($"[SAGA] Board {boardId} ki {cards.Count} cards delete hui.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task CompactPositions(int listId)
        {
            var activeCards = await _cardRepository.FindByListIdOrderByPosition(listId);
            for (int i = 0; i < activeCards.Count; i++)
                activeCards[i].Position = i + 1;

            if (activeCards.Any())
                await _cardRepository.UpdateRangePositions(activeCards);
        }
    }
}