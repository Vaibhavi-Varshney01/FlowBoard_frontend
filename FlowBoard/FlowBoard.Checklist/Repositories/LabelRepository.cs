using FlowBoard.Checklist.Infrastructure;
using FlowBoard.Checklist.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Checklist.Repositories
{
    public class LabelRepository : ILabelRepository
    {
        private readonly ChecklistDbContext _context;

        public LabelRepository(ChecklistDbContext context)
        {
            _context = context;
        }

        // ── Label ───────────────────────────────────────────────────────

        public Label CreateLabel(Label label)
        {
            _context.Labels.Add(label);
            Save();
            return label;
        }

        public Label GetLabelById(int labelId) =>
            _context.Labels.FirstOrDefault(l => l.LabelId == labelId)
            ?? throw new KeyNotFoundException($"Label {labelId} not found.");

        public List<Label> GetLabelsByBoard(int boardId) =>
            _context.Labels.Where(l => l.BoardId == boardId).ToList();

        public Label UpdateLabel(Label label)
        {
            _context.Labels.Update(label);
            Save();
            return label;
        }

        public void DeleteLabel(int labelId)
        {
            var label = GetLabelById(labelId);
            _context.Labels.Remove(label);
            Save();
        }

        // ── Card–Label associations (in-memory join) ────────────────────

        private readonly List<(int CardId, int LabelId)> _cardLabels = new();

        public void AddLabelToCard(int cardId, int labelId)
        {
            if (!_cardLabels.Any(cl => cl.CardId == cardId && cl.LabelId == labelId))
                _cardLabels.Add((cardId, labelId));
        }

        public void RemoveLabelFromCard(int cardId, int labelId) =>
            _cardLabels.RemoveAll(cl => cl.CardId == cardId && cl.LabelId == labelId);

        public List<Label> GetLabelsForCard(int cardId)
        {
            var labelIds = _cardLabels
                .Where(cl => cl.CardId == cardId)
                .Select(cl => cl.LabelId)
                .ToHashSet();

            return _context.Labels.Where(l => labelIds.Contains(l.LabelId)).ToList();
        }

        // ── Checklist ───────────────────────────────────────────────────

        public TaskChecklist CreateChecklist(TaskChecklist checklist)
        {
            _context.Checklists.Add(checklist);
            Save();
            return checklist;
        }

        public TaskChecklist GetChecklistById(int checklistId) =>
            _context.Checklists.FirstOrDefault(c => c.ChecklistId == checklistId)
            ?? throw new KeyNotFoundException($"Checklist {checklistId} not found.");

        public List<TaskChecklist> GetChecklistsByCard(int cardId) =>
            _context.Checklists.Where(c => c.CardId == cardId).ToList();

        public void DeleteChecklist(int checklistId)
        {
            var checklist = GetChecklistById(checklistId);
            _context.Checklists.Remove(checklist);
            Save();
        }

        // ── ChecklistItem ───────────────────────────────────────────────

        public ChecklistItem AddItem(ChecklistItem item)
        {
            _context.ChecklistItems.Add(item);
            Save();
            return item;
        }

        public ChecklistItem GetItemById(int itemId) =>
            _context.ChecklistItems.FirstOrDefault(i => i.ItemId == itemId)
            ?? throw new KeyNotFoundException($"ChecklistItem {itemId} not found.");

        public List<ChecklistItem> GetItemsByChecklist(int checklistId) =>
            _context.ChecklistItems.Where(i => i.ChecklistId == checklistId).ToList();

        public void UpdateItem(ChecklistItem item)
        {
            _context.ChecklistItems.Update(item);
            Save();
        }

        public void DeleteItem(int itemId)
        {
            var item = GetItemById(itemId);
            _context.ChecklistItems.Remove(item);
            Save();
        }

        // ── Persistence ─────────────────────────────────────────────────

        public void Save() => _context.SaveChanges();
    }
}