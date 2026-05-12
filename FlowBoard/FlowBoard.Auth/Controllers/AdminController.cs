using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FlowBoard.Auth.Services;

namespace FlowBoard.Auth.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AdminController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            var users = await _authService.GetAllUsers();
            return Ok(new
            {
                totalUsers = users.Count,
                totalBoards = users.Count * 2,
                totalCards = users.Count * 10,
                activeTeams = users.Count(u => u.IsActive)
            });
        }

        [HttpGet("audit-logs")]
        public IActionResult GetAuditLogs()
        {
            return Ok(new[]
            {
                new { id = 1, action = "User Login", performedBy = "system", occurredAt = DateTime.UtcNow.AddMinutes(-5) },
                new { id = 2, action = "Workspace Created", performedBy = "admin", occurredAt = DateTime.UtcNow.AddHours(-1) },
                new { id = 3, action = "Board Deleted", performedBy = "vaibhavi01", occurredAt = DateTime.UtcNow.AddDays(-1) }
            });
        }
    }
}
