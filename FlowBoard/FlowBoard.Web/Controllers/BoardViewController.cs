using Microsoft.AspNetCore.Mvc;
using FlowBoard.Web.Services;
using FlowBoard.Web.DTOs;
using FlowBoard.Web.ViewModels;

namespace FlowBoard.Web.Controllers
{
    // FlowBoard Web Controllers — MVC Layer
    // Calls microservices via IHttpClientFactory (replaces RestTemplate)
    public class BoardViewController : Controller
    {
        private readonly IAuthService         _authService;
        private readonly IWorkspaceService    _wsService;
        private readonly IBoardService        _boardService;
        private readonly IListService         _listService;
        private readonly ICardService         _cardService;
        private readonly INotificationService _notifService;

        public BoardViewController(
            IAuthService authService,
            IWorkspaceService wsService,
            IBoardService boardService,
            IListService listService,
            ICardService cardService,
            INotificationService notifService)
        {
            _authService  = authService;
            _wsService    = wsService;
            _boardService = boardService;
            _listService  = listService;
            _cardService  = cardService;
            _notifService = notifService;
        }

        private string? Token => HttpContext.Session.GetString("jwt");
        private int     UserId => int.Parse(HttpContext.Session.GetString("userId") ?? "0");

        // GET /
        [HttpGet("/")]
        public IActionResult Home() => View();

        // GET /register
        [HttpGet("/register")]
        public IActionResult Register() => View();

        // POST /register
        [HttpPost("/register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var success = await _authService.RegisterAsync(dto);
            if (!success) { ViewBag.Error = "Registration failed."; return View(dto); }
            return RedirectToAction(nameof(Login));
        }

        // GET /login
        [HttpGet("/login")]
        public IActionResult Login() => View();

        // POST /login
        [HttpPost("/login")]
        public async Task<IActionResult> LoginString(string email, string password, System.Security.Claims.ClaimsPrincipal principal)
        {
            var token = await _authService.LoginAsync(new LoginDto { Email = email, Password = password });
            if (token == null) { ViewBag.Error = "Invalid credentials."; return View("Login"); }
            HttpContext.Session.SetString("jwt", token);
            return RedirectToAction(nameof(ViewDashboard));
        }

        // GET /dashboard
        [HttpGet("/dashboard")]
        public async Task<IActionResult> ViewDashboard()
        {
            if (Token == null) return RedirectToAction(nameof(Login));

            var workspaces   = await _wsService.GetByMemberAsync(UserId, Token);
            var recentBoards = await _boardService.GetByMemberAsync(UserId, Token);
            var unreadCount  = await _notifService.GetUnreadCountAsync(UserId, Token);

            var vm = new DashboardViewModel
            {
                Workspaces   = workspaces,
                RecentBoards = recentBoards,
                UnreadCount  = unreadCount
            };

            return View(vm);
        }

        // GET /workspaces/{id}
        [HttpGet("/workspaces/{id}")]
        public async Task<IActionResult> ViewWorkspace(int id)
        {
            if (Token == null) return RedirectToAction(nameof(Login));
            var ws = await _wsService.GetWorkspaceAsync(id, Token);
            if (ws == null) return NotFound();
            return View(ws);
        }

        // POST /workspaces/create
        [HttpPost("/workspaces/create")]
        public async Task<IActionResult> CreateWorkspace(CreateWorkspaceDto dto)
        {
            if (Token == null) return RedirectToAction(nameof(Login));
            await _wsService.CreateWorkspaceAsync(dto, Token);
            return RedirectToAction(nameof(ViewDashboard));
        }

        // POST /workspaces/{id}/edit
        [HttpPost("/workspaces/{id}/edit")]
        public async Task<IActionResult> EditWorkspace(int id, CreateWorkspaceDto dto)
        {
            if (Token == null) return RedirectToAction(nameof(Login));
            await _wsService.UpdateWorkspaceAsync(id, dto, Token);
            return RedirectToAction(nameof(ViewWorkspace), new { id });
        }

        // POST /workspaces/{id}/delete
        [HttpPost("/workspaces/{id}/delete")]
        public async Task<IActionResult> DeleteWorkspace(int id)
        {
            if (Token == null) return RedirectToAction(nameof(Login));
            await _wsService.DeleteWorkspaceAsync(id, Token);
            return RedirectToAction(nameof(ViewDashboard));
        }

        // POST /workspaces/{id}/members/add
        [HttpPost("/workspaces/{id}/members/add")]
        public async Task<IActionResult> AddWorkspaceMember(int id, int userId, string role)
        {
            if (Token == null) return RedirectToAction(nameof(Login));
            await _wsService.AddMemberAsync(id, userId, role, Token);
            return RedirectToAction(nameof(ViewWorkspace), new { id });
        }

        // GET /boards/{id}
        [HttpGet("/boards/{id}")]
        public async Task<IActionResult> ViewBoard(int id)
        {
            if (Token == null) return RedirectToAction(nameof(Login));
            var board = await _boardService.GetBoardAsync(id, Token);
            if (board == null) return NotFound();
            return View(board);
        }

        // POST /boards/create
        [HttpPost("/boards/create")]
        public async Task<IActionResult> CreateBoard(CreateBoardDto dto)
        {
            if (Token == null) return RedirectToAction(nameof(Login));
            await _boardService.CreateBoardAsync(dto, Token);
            return RedirectToAction(nameof(ViewDashboard));
        }

        // POST /boards/{id}/update
        [HttpPost("/boards/{id}/update")]
        public async Task<IActionResult> UpdateBoard(int id, CreateBoardDto dto)
        {
            if (Token == null) return RedirectToAction(nameof(Login));
            await _boardService.UpdateBoardAsync(id, dto, Token);
            return RedirectToAction(nameof(ViewBoard), new { id });
        }

        // POST /boards/{id}/close
        [HttpPost("/boards/{id}/close")]
        public async Task<IActionResult> CloseBoard(int id)
        {
            if (Token == null) return RedirectToAction(nameof(Login));
            await _boardService.CloseBoardAsync(id, Token);
            return RedirectToAction(nameof(ViewDashboard));
        }

        // POST /boards/{id}/members/add
        [HttpPost("/boards/{id}/members/add")]
        public async Task<IActionResult> AddBoardMember(int id, int userId, string role)
        {
            if (Token == null) return RedirectToAction(nameof(Login));
            await _boardService.AddMemberAsync(id, userId, role, Token);
            return RedirectToAction(nameof(ViewBoard), new { id });
        }

        // GET /notifications
        [HttpGet("/notifications")]
        public async Task<IActionResult> ViewNotifications()
        {
            if (Token == null) return RedirectToAction(nameof(Login));
            var notifications = await _notifService.GetByRecipientAsync(UserId, Token);
            return View(notifications);
        }

        // POST /notifications/{id}/read
        [HttpPost("/notifications/{id}/read")]
        public async Task<IActionResult> MarkNotifRead(int id)
        {
            if (Token == null) return RedirectToAction(nameof(Login));
            await _notifService.MarkAsReadAsync(id, Token);
            return RedirectToAction(nameof(ViewNotifications));
        }

        // POST /logout
        [HttpPost("/logout")]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync(HttpContext);
            return RedirectToAction(nameof(Home));
        }
    }
}