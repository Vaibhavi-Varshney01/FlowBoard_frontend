using Microsoft.AspNetCore.Mvc;
using FlowBoard.Checklist.Models;
using FlowBoard.Checklist.Services;

namespace FlowBoard.Checklist.Controllers
{
    [ApiController]
    public class LabelController : ControllerBase
    {
        private readonly ILabelService _labelService;

        public LabelController(ILabelService labelService)
        {
            _labelService = labelService;
        }

        // ════════════════════════════════════════════════════
        // /api/labels
        // ════════════════════════════════════════════════════

        // POST /api/labels
        [HttpPost("api/labels")]
        public IActionResult CreateLabel([FromBody] Label label)
        {
            var created = _labelService.CreateLabel(label);
            return CreatedAtAction(nameof(GetLabelById), new { id = created.LabelId }, created);
        }

        // GET /api/labels/{id}
        [HttpGet("api/labels/{id:int}")]
        public IActionResult GetLabelById(int id)
        {
            try { return Ok(_labelService.GetLabelById(id)); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // GET /api/labels/board/{boardId}
        [HttpGet("api/labels/board/{boardId:int}")]
        public ActionResult<List<Label>> GetByBoard(int boardId) =>
            Ok(_labelService.GetLabelsByBoard(boardId));

        // PUT /api/labels/{id}
        [HttpPut("api/labels/{id:int}")]
        public IActionResult UpdateLabel(int id, [FromBody] Label label)
        {
            try { return Ok(_labelService.UpdateLabel(id, label)); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // DELETE /api/labels/{id}
        [HttpDelete("api/labels/{id:int}")]
        public IActionResult DeleteLabel(int id)
        {
            try { _labelService.DeleteLabel(id); return NoContent(); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // ── Card–Label  ──────────────────────────────────────

        // POST /api/labels/card/{cardId}/{labelId}
        [HttpPost("api/labels/card/{cardId:int}/{labelId:int}")]
        public IActionResult AddToCard(int cardId, int labelId)
        {
            _labelService.AddLabelToCard(cardId, labelId);
            return NoContent();
        }

        // DELETE /api/labels/card/{cardId}/{labelId}
        [HttpDelete("api/labels/card/{cardId:int}/{labelId:int}")]
        public IActionResult RemoveFromCard(int cardId, int labelId)
        {
            _labelService.RemoveLabelFromCard(cardId, labelId);
            return NoContent();
        }

        // GET /api/labels/card/{cardId}
        [HttpGet("api/labels/card/{cardId:int}")]
        public ActionResult<List<Label>> GetForCard(int cardId) =>
            Ok(_labelService.GetLabelsForCard(cardId));

        // ════════════════════════════════════════════════════
        // /api/checklists
        // ════════════════════════════════════════════════════

        // POST /api/checklists
        [HttpPost("api/checklists")]
        public IActionResult CreateChecklist([FromBody] TaskChecklist checklist)
        {
            var created = _labelService.CreateChecklist(checklist);
            return CreatedAtAction(nameof(GetChecklistsByCard),
                new { cardId = created.CardId }, created);
        }

        // GET /api/checklists/card/{cardId}
        [HttpGet("api/checklists/card/{cardId:int}")]
        public ActionResult<List<TaskChecklist>> GetChecklistsByCard(int cardId) =>
            Ok(_labelService.GetChecklistsByCard(cardId));

        // DELETE /api/checklists/{id}
        [HttpDelete("api/checklists/{id:int}")]
        public IActionResult DeleteChecklist(int id)
        {
            try { _labelService.DeleteChecklist(id); return NoContent(); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // ── Items ────────────────────────────────────────────

        // POST /api/checklists/{checklistId}/items
        [HttpPost("api/checklists/{checklistId:int}/items")]
        public IActionResult AddItem(int checklistId, [FromBody] ChecklistItem item)
        {
            var created = _labelService.AddItem(checklistId, item);
            return Ok(created);
        }

        // PUT /api/checklists/items/{itemId}/toggle
        [HttpPut("api/checklists/items/{itemId:int}/toggle")]
        public IActionResult ToggleItem(int itemId)
        {
            try { _labelService.ToggleItem(itemId); return NoContent(); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // ── Progress ─────────────────────────────────────────

        // GET /api/checklists/{checklistId}/progress
        [HttpGet("api/checklists/{checklistId:int}/progress")]
        public ActionResult<double> GetChecklistProgress(int checklistId) =>
            Ok(_labelService.GetChecklistProgress(checklistId));
    }
}