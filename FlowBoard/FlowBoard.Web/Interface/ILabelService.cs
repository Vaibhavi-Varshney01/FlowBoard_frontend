namespace FlowBoard.Web.Services
{
    public interface ILabelService
    {
        Task<bool>         AddLabelToCardAsync(int cardId, int labelId, string token);
        Task<bool>         RemoveLabelFromCardAsync(int cardId, int labelId, string token);
        Task<List<object>> GetLabelsForCardAsync(int cardId, string token);
        Task<object?>      CreateChecklistAsync(object model, string token);
        Task<bool>         ToggleChecklistItemAsync(int itemId, string token);
        Task<double>       GetChecklistProgressAsync(int checklistId, string token);
    }
}