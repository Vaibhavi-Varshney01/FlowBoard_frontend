using FlowBoard.Checklist.Models;

namespace FlowBoard.Checklist.Services
{
    public interface ILabelService
    {
        // ── Label CRUD ──────────────────────────────────────────────────
        Label CreateLabel(Label label);
        List<Label> GetLabelsByBoard(int boardId);
        Label GetLabelById(int labelId);
        Label UpdateLabel(int labelId, Label label);
        void DeleteLabel(int labelId);

        // ── Card–Label associations ─────────────────────────────────────
        void AddLabelToCard(int cardId, int labelId);
        void RemoveLabelFromCard(int cardId, int labelId);
        List<Label> GetLabelsForCard(int cardId);

        // ── Checklist CRUD ──────────────────────────────────────────────
        TaskChecklist CreateChecklist(TaskChecklist checklist);
        List<TaskChecklist> GetChecklistsByCard(int cardId);
        void DeleteChecklist(int checklistId);

        // ── ChecklistItem operations ────────────────────────────────────
        ChecklistItem AddItem(int checklistId, ChecklistItem item);
        void ToggleItem(int itemId);

        // ── Progress ────────────────────────────────────────────────────
        double GetChecklistProgress(int checklistId);
    }
}