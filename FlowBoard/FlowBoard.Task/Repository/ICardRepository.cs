using FlowBoard.Card.Models;

namespace FlowBoard.Card.Repositories
{
    public interface ICardRepository
    {
        Task<List<Models.Card>> FindByListId(int listId);
        Task<List<Models.Card>> FindByBoardId(int boardId);
        Task<List<Models.Card>> FindByAssigneeId(string assigneeId);
        Task<Models.Card?> FindByCardId(int cardId);
        Task<List<Models.Card>> FindByListIdOrderByPosition(int listId);
        Task<List<Models.Card>> FindByDueDateBefore(DateTime cutoff);
        Task<List<Models.Card>> FindByPriority(Priority priority);
        Task<List<Models.Card>> FindByStatus(CardStatus status);
        Task<int> CountByListId(int listId);

        Task<Models.Card> Save(Models.Card card);
        Task<Models.Card> Update(Models.Card card);
        Task DeleteByCardId(int cardId);
        Task UpdateRangePositions(List<Models.Card> cards);

        // Activity feed
        Task LogActivity(CardActivity activity);
        Task<List<CardActivity>> GetActivitiesByCardId(int cardId);
    }
}