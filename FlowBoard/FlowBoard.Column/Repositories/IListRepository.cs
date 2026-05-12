using FlowBoard.List.Models;

namespace FlowBoard.List.Repositories
{
    public interface IListRepository
    {
        Task<List<TaskList>> FindByBoardId(int boardId);
        Task<TaskList?> FindByListId(int listId);
        Task<List<TaskList>> FindByBoardIdOrderByPosition(int boardId);
        Task<List<TaskList>> FindByBoardIdAndIsArchived(int boardId, bool isArchived);
        Task<int> CountByBoardId(int boardId);
        Task<int> FindMaxPositionByBoardId(int boardId);
        Task DeleteByListId(int listId);

        Task<TaskList> Save(TaskList list);
        Task<TaskList> Update(TaskList list);
        Task UpdateRangePositions(List<TaskList> lists);
    }
}