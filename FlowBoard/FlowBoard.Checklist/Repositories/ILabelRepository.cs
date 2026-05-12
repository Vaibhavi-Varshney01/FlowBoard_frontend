using FlowBoard.Checklist.Models;

namespace FlowBoard.Checklist.Repositories
{
    public interface ILabelRepository
    {
        // ── Label ───────────────────────────────────────────────────────
        Label CreateLabel(Label label);
        Label GetLabelById(int labelId);
        List<Label> GetLabelsByBoard(int boardId);
        Label UpdateLabel(Label label);
        void DeleteLabel(int labelId);

        // ── Card–Label associations ─────────────────────────────────────
        void AddLabelToCard(int cardId, int labelId);
        void RemoveLabelFromCard(int cardId, int labelId);
        List<Label> GetLabelsForCard(int cardId);

        // ── Checklist ───────────────────────────────────────────────────
        TaskChecklist CreateChecklist(TaskChecklist checklist);
        TaskChecklist GetChecklistById(int checklistId);
        List<TaskChecklist> GetChecklistsByCard(int cardId);
        void DeleteChecklist(int checklistId);

        // ── ChecklistItem ───────────────────────────────────────────────
        ChecklistItem AddItem(ChecklistItem item);
        ChecklistItem GetItemById(int itemId);
        List<ChecklistItem> GetItemsByChecklist(int checklistId);
        void UpdateItem(ChecklistItem item);
        void DeleteItem(int itemId);

        // ── Persistence ─────────────────────────────────────────────────
        void Save();
    }
}