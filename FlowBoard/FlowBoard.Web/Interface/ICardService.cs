using FlowBoard.Web.DTOs;

namespace FlowBoard.Web.Services
{
    public interface ICardService
    {
        Task<object?>      GetCardAsync(int id, string token);
        Task<List<object>> GetByListAsync(int listId, string token);
        Task<List<object>> GetByBoardAsync(int boardId, string token);
        Task<List<object>> GetByAssigneeAsync(int userId, string token);
        Task<List<object>> GetOverdueCardsAsync(string token);
        Task<object?>      CreateCardAsync(CreateCardDto dto, string token);
        Task<bool>         UpdateCardAsync(int id, object model, string token);
        Task<bool>         MoveCardAsync(int id, int targetListId, int position, string token);
        Task<bool>         ReorderCardsAsync(int listId, List<int> cardIds, string token);
        Task<bool>         ArchiveCardAsync(int id, string token);
        Task<bool>         UnarchiveCardAsync(int id, string token);
        Task<bool>         DeleteCardAsync(int id, string token);
        Task<bool>         SetAssigneeAsync(int id, int assigneeId, string token);
        Task<bool>         SetPriorityAsync(int id, string priority, string token);
    }
}