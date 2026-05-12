using FlowBoard.List.Models;

namespace FlowBoard.List.Services
{
    public record CreateListRequest(
        int BoardId,
        string Name,
        string? Color
    );

    public record UpdateListRequest(
        string Name,
        string? Color
    );

    public record ReorderListsRequest(
        List<ListPositionEntry> Positions
    );

    public record ListPositionEntry(
        int ListId,
        int Position
    );

    public record MoveListRequest(
        int TargetBoardId
    );

    public interface IListService
    {
        // CRUD
        Task<TaskList> CreateList(CreateListRequest request);
        Task<TaskList> GetListById(int listId);
        Task<List<TaskList>> GetListsByBoard(int boardId);
        Task<TaskList> UpdateList(int listId, UpdateListRequest request);
        Task DeleteList(int listId);

        // Position management
        Task<List<TaskList>> ReorderLists(int boardId, ReorderListsRequest request);

        // Archival
        Task<TaskList> ArchiveList(int listId);
        Task<TaskList> UnarchiveList(int listId);
        Task<List<TaskList>> GetArchivedLists(int boardId);

        // Board transfer
        Task<TaskList> MoveList(int listId, MoveListRequest request);

        // SAGA: Board delete hone pe saari lists delete karo
        Task DeleteAllListsByBoard(int boardId);
    }
}