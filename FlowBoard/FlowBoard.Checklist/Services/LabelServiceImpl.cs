using FlowBoard.Checklist.Models;
using FlowBoard.Checklist.Repositories;

namespace FlowBoard.Checklist.Services
{
    public class LabelServiceImpl : ILabelService
    {
        private readonly ILabelRepository _repo;

        public LabelServiceImpl(ILabelRepository repo)
        {
            _repo = repo;
        }

        // ── Label CRUD ──────────────────────────────────────────────────

        public Label CreateLabel(Label label) =>
            _repo.CreateLabel(label);

        public List<Label> GetLabelsByBoard(int boardId) =>
            _repo.GetLabelsByBoard(boardId);

        public Label GetLabelById(int labelId) =>
            _repo.GetLabelById(labelId);

        public Label UpdateLabel(int labelId, Label label)
        {
            var existing = _repo.GetLabelById(labelId);
            existing.Name  = label.Name;
            existing.Color = label.Color;
            return _repo.UpdateLabel(existing);
        }

        public void DeleteLabel(int labelId) =>
            _repo.DeleteLabel(labelId);

        // ── Card–Label associations ─────────────────────────────────────

        public void AddLabelToCard(int cardId, int labelId) =>
            _repo.AddLabelToCard(cardId, labelId);

        public void RemoveLabelFromCard(int cardId, int labelId) =>
            _repo.RemoveLabelFromCard(cardId, labelId);

        public List<Label> GetLabelsForCard(int cardId) =>
            _repo.GetLabelsForCard(cardId);

        // ── Checklist CRUD ──────────────────────────────────────────────

        public TaskChecklist CreateChecklist(TaskChecklist checklist) =>
            _repo.CreateChecklist(checklist);

        public List<TaskChecklist> GetChecklistsByCard(int cardId) =>
            _repo.GetChecklistsByCard(cardId);

        public void DeleteChecklist(int checklistId) =>
            _repo.DeleteChecklist(checklistId);

        // ── ChecklistItem operations ────────────────────────────────────

        public ChecklistItem AddItem(int checklistId, ChecklistItem item)
        {
            item.ChecklistId = checklistId;
            return _repo.AddItem(item);
        }

        public void ToggleItem(int itemId)
        {
            var item = _repo.GetItemById(itemId);
            item.IsCompleted = !item.IsCompleted;
            _repo.UpdateItem(item);
        }

        // ── Progress ────────────────────────────────────────────────────

        public double GetChecklistProgress(int checklistId)
        {
            var items = _repo.GetItemsByChecklist(checklistId);

            int total = items.Count;
            if (total == 0) return 0.0;

            int completed = items.Count(i => i.IsCompleted);
            return Math.Round((double)completed / total * 100.0, 2);
        }
    }
}