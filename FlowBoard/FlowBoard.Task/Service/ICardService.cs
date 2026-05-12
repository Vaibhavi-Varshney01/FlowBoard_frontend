using FlowBoard.Card.Models;

namespace FlowBoard.Card.Services
{
    // ── Request / Entry DTOs ─────────────────────────────────────────────────

    public record CreateCardRequest(
        int    ListId,
        int    BoardId,
        string Title,
        string? Description,
        Priority Priority,
        CardStatus Status,
        DateTime? DueDate,
        DateTime? StartDate,
        string? AssigneeId,
        string CreatedById,
        string? CoverColor
    );

    public record UpdateCardRequest(
        string  Title,
        string? Description,
        Priority Priority,
        CardStatus Status,
        DateTime? DueDate,
        DateTime? StartDate,
        string? CoverColor
    );

    public record MoveCardRequest(
        int TargetListId,
        int TargetBoardId,
        int TargetPosition
    );

    public record ReorderCardsRequest(
        List<CardPositionEntry> Positions
    );

    public record CardPositionEntry(
        int CardId,
        int Position
    );

    public record SetAssigneeRequest(
        string? AssigneeId   // null = unassign
    );

    public record SetPriorityRequest(
        Priority Priority
    );

    // ── Filter DTO ───────────────────────────────────────────────────────────

    public record CardFilterRequest(
        string?    AssigneeId,
        DateTime?  DueDateBefore,
        Priority?  Priority,
        CardStatus? Status
    );

    // ── Service Interface ────────────────────────────────────────────────────

    public interface ICardService
    {
        // CRUD
        Task<Models.Card> CreateCard(CreateCardRequest request);
        Task<Models.Card> GetCardById(int cardId);
        Task<List<Models.Card>> GetCardsByList(int listId);
        Task<List<Models.Card>> GetCardsByBoard(int boardId);
        Task<List<Models.Card>> GetCardsByAssignee(string assigneeId);
        Task<Models.Card> UpdateCard(int cardId, UpdateCardRequest request, string actorId);
        Task DeleteCard(int cardId);

        // Move & Reorder
        Task<Models.Card> MoveCard(int cardId, MoveCardRequest request, string actorId);
        Task<List<Models.Card>> ReorderCards(int listId, ReorderCardsRequest request);

        // Archival
        Task<Models.Card> ArchiveCard(int cardId, string actorId);
        Task<Models.Card> UnarchiveCard(int cardId, string actorId);

        // Assignment & Priority
        Task<Models.Card> SetAssignee(int cardId, SetAssigneeRequest request, string actorId);
        Task<Models.Card> SetPriority(int cardId, SetPriorityRequest request, string actorId);

        // Overdue detection
        Task<List<Models.Card>> GetOverdueCards();

        // Activity feed
        Task<List<CardActivity>> GetCardActivity(int cardId);

        // Multi-criteria filter
        Task<List<Models.Card>> FilterCards(CardFilterRequest filter);

        // SAGA: list or board deleted
        Task DeleteAllCardsByList(int listId);
        Task DeleteAllCardsByBoard(int boardId);
    }
}