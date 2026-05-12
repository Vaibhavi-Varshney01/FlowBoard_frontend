using Microsoft.AspNetCore.Mvc;
using FlowBoard.Web.Services;
using FlowBoard.Web.DTOs;

namespace FlowBoard.Web.Controllers
{
    public class CardController : Controller
    {
        private readonly ICardService         _cardService;
        private readonly IListService         _listService;
        private readonly ICommentService      _commentService;
        private readonly IBoardService        _boardService;
        private readonly ILabelService        _labelService;
        private readonly INotificationService _notifService;

        public CardController(
            ICardService cardService,
            IListService listService,
            ICommentService commentService,
            IBoardService boardService,
            ILabelService labelService,
            INotificationService notifService)
        {
            _cardService    = cardService;
            _listService    = listService;
            _commentService = commentService;
            _boardService   = boardService;
            _labelService   = labelService;
            _notifService   = notifService;
        }

        private string? Token  => HttpContext.Session.GetString("jwt");
        private int     UserId => int.Parse(HttpContext.Session.GetString("userId") ?? "0");

        // GET /cards/{id}
        [HttpGet("/cards/{id}")]
        public async Task<IActionResult> ViewCard(int id)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            var card = await _cardService.GetCardAsync(id, Token);
            if (card == null) return NotFound();
            return View(card);
        }

        // POST /cards/create
        [HttpPost("/cards/create")]
        public async Task<IActionResult> CreateCard(CreateCardDto dto)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _cardService.CreateCardAsync(dto, Token);
            return RedirectToAction("ViewBoard", "BoardView", new { id = dto.BoardId });
        }

        // POST /cards/{id}/edit
        [HttpPost("/cards/{id}/edit")]
        public async Task<IActionResult> EditCard(int id, CreateCardDto dto)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _cardService.UpdateCardAsync(id, dto, Token);
            return RedirectToAction(nameof(ViewCard), new { id });
        }

        // POST /cards/{id}/move
        [HttpPost("/cards/{id}/move")]
        public async Task<IActionResult> MoveCard(int id, int targetListId, int position)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _cardService.MoveCardAsync(id, targetListId, position, Token);
            return RedirectToAction(nameof(ViewCard), new { id });
        }

        // POST /cards/{id}/archive
        [HttpPost("/cards/{id}/archive")]
        public async Task<IActionResult> ArchiveCard(int id)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _cardService.ArchiveCardAsync(id, Token);
            return RedirectToAction(nameof(ViewCard), new { id });
        }

        // POST /cards/{id}/delete
        [HttpPost("/cards/{id}/delete")]
        public async Task<IActionResult> DeleteCard(int id)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _cardService.DeleteCardAsync(id, Token);
            return RedirectToAction("ViewDashboard", "BoardView");
        }

        // POST /cards/{id}/assignee
        [HttpPost("/cards/{id}/assignee")]
        public async Task<IActionResult> SetAssignee(int id, int assigneeId)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _cardService.SetAssigneeAsync(id, assigneeId, Token);
            return RedirectToAction(nameof(ViewCard), new { id });
        }

        // POST /cards/{id}/priority
        [HttpPost("/cards/{id}/priority")]
        public async Task<IActionResult> SetPriority(int id, string priority)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _cardService.SetPriorityAsync(id, priority, Token);
            return RedirectToAction(nameof(ViewCard), new { id });
        }

        // POST /lists/create
        [HttpPost("/lists/create")]
        public async Task<IActionResult> CreateList(object taskList, int boardId)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _listService.CreateListAsync(taskList, Token);
            return RedirectToAction("ViewBoard", "BoardView", new { id = boardId });
        }

        // POST /lists/{boardId}/reorder
        [HttpPost("/lists/{boardId}/reorder")]
        public async Task<IActionResult> ReorderLists(int boardId, List<int> listIds)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _listService.ReorderListsAsync(boardId, listIds, Token);
            return Ok();
        }

        // POST /cards/{boardId}/reorder
        [HttpPost("/cards/{boardId}/reorder")]
        public async Task<IActionResult> ReorderCards(int boardId, List<int> cardIds)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _cardService.ReorderCardsAsync(boardId, cardIds, Token);
            return Ok();
        }

        // POST /cards/{id}/comments/add
        [HttpPost("/cards/{id}/comments/add")]
        public async Task<IActionResult> AddComment(int id, string content)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _commentService.AddCommentAsync(id, content, Token);
            return RedirectToAction(nameof(ViewCard), new { id });
        }

        // POST /comments/{id}/edit
        [HttpPost("/comments/{id}/edit")]
        public async Task<IActionResult> EditComment(int id, string content)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _commentService.EditCommentAsync(id, content, Token);
            return RedirectToAction(nameof(ViewCard));
        }

        // POST /comments/{id}/delete
        [HttpPost("/comments/{id}/delete")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _commentService.DeleteCommentAsync(id, Token);
            return RedirectToAction(nameof(ViewCard));
        }

        // POST /cards/{id}/attachments/add
        [HttpPost("/cards/{id}/attachments/add")]
        public async Task<IActionResult> AddAttachment(int id, IFormFile formFile)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _commentService.AddAttachmentAsync(id, formFile, Token);
            return RedirectToAction(nameof(ViewCard), new { id });
        }

        // POST /attachments/{id}/delete
        [HttpPost("/attachments/{id}/delete")]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _commentService.DeleteAttachmentAsync(id, Token);
            return RedirectToAction(nameof(ViewCard));
        }

        // POST /cards/{id}/labels/add
        [HttpPost("/cards/{id}/labels/add")]
        public async Task<IActionResult> AddLabel(int id, int labelId)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _labelService.AddLabelToCardAsync(id, labelId, Token);
            return RedirectToAction(nameof(ViewCard), new { id });
        }

        // POST /cards/{id}/checklists/create
        [HttpPost("/cards/{id}/checklists/create")]
        public async Task<IActionResult> CreateChecklist(int id, object checklist)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _labelService.CreateChecklistAsync(checklist, Token);
            return RedirectToAction(nameof(ViewCard), new { id });
        }

        // POST /checklists/items/{itemId}/toggle
        [HttpPost("/checklists/items/{itemId}/toggle")]
        public async Task<IActionResult> ToggleChecklistItem(int itemId)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _labelService.ToggleChecklistItemAsync(itemId, Token);
            return RedirectToAction(nameof(ViewCard));
        }
    }
}