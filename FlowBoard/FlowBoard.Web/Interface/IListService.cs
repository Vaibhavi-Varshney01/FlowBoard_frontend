namespace FlowBoard.Web.Services
{
    public interface IListService
    {
        Task<object?> CreateListAsync(object model, string token);
        Task<bool>    ReorderListsAsync(int boardId, List<int> listIds, string token);
    }
}