using Microsoft.AspNetCore.Mvc;
using FlowBoard.Web.Services;

namespace FlowBoard.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthService         _authService;
        private readonly IWorkspaceService    _wsService;
        private readonly IBoardService        _boardService;
        private readonly ICardService         _cardService;
        private readonly INotificationService _notifService;

        public AdminController(
            IAuthService authService,
            IWorkspaceService wsService,
            IBoardService boardService,
            ICardService cardService,
            INotificationService notifService)
        {
            _authService  = authService;
            _wsService    = wsService;
            _boardService = boardService;
            _cardService  = cardService;
            _notifService = notifService;
        }

        private string? Token => HttpContext.Session.GetString("jwt");

        // GET /admin
        [HttpGet("/admin")]
        public IActionResult AdminDashboard() => View();

        // GET /admin/users
        [HttpGet("/admin/users")]
        public async Task<IActionResult> ManageAllUsers()
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            var users = await _authService.SearchUsersAsync("");
            return View(users);
        }

        // POST /admin/users/{id}/suspend
        [HttpPost("/admin/users/{id}/suspend")]
        public async Task<IActionResult> SuspendUser(int id)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _authService.DeactivateAccountAsync(Token, id);
            return RedirectToAction(nameof(ManageAllUsers));
        }

        // POST /admin/users/{id}/delete
        [HttpPost("/admin/users/{id}/delete")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _authService.DeactivateAccountAsync(Token, id);
            return RedirectToAction(nameof(ManageAllUsers));
        }

        // GET /admin/workspaces
        [HttpGet("/admin/workspaces")]
        public async Task<IActionResult> ManageAllWorkspaces()
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            var all = await _wsService.GetByOwnerAsync(0, Token);
            return View(all);
        }

        // POST /admin/workspaces/{id}/delete
        [HttpPost("/admin/workspaces/{id}/delete")]
        public async Task<IActionResult> DeleteWorkspace(int id)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _wsService.DeleteWorkspaceAsync(id, Token);
            return RedirectToAction(nameof(ManageAllWorkspaces));
        }

        // GET /admin/boards
        [HttpGet("/admin/boards")]
        public async Task<IActionResult> ManageAllBoards()
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            var boards = await _boardService.GetByMemberAsync(0, Token);
            return View(boards);
        }

        // POST /admin/boards/{id}/delete
        [HttpPost("/admin/boards/{id}/delete")]
        public async Task<IActionResult> DeleteBoard(int id)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _boardService.DeleteBoardAsync(id, Token);
            return RedirectToAction(nameof(ManageAllBoards));
        }

        // GET /admin/analytics
        [HttpGet("/admin/analytics")]
        public IActionResult ViewPlatformAnalytics() => View();

        // GET /admin/cards
        [HttpGet("/admin/cards")]
        public async Task<IActionResult> ViewAllCards()
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            var cards = await _cardService.GetByBoardAsync(0, Token);
            return View(cards);
        }

        // GET /admin/cards/overdue
        [HttpGet("/admin/cards/overdue")]
        public async Task<IActionResult> ViewOverdueCards()
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            var overdue = await _cardService.GetOverdueCardsAsync(Token);
            return View(overdue);
        }

        // POST /admin/notifications/send
        [HttpPost("/admin/notifications/send")]
        public async Task<IActionResult> SendPlatformNotification(string message, string targetGroup)
        {
            if (Token == null) return RedirectToAction("Login", "BoardView");
            await _notifService.SendBulkAsync(new { Message = message, TargetGroup = targetGroup }, Token);
            return RedirectToAction(nameof(AdminDashboard));
        }

        // GET /admin/audit
        [HttpGet("/admin/audit")]
        public IActionResult ViewAuditLogs() => View();

        // GET /admin/reports/activity
        [HttpGet("/admin/reports/activity")]
        public IActionResult GenerateActivityReport() => View();
    }
}