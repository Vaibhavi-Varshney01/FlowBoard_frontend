using FlowBoard.Auth.Models;
using FlowBoard.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FlowBoard.Auth.DTOs;

namespace FlowBoard.Auth.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.Register(request);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.Login(request.Email, request.Password, request.Role);
            return Ok(response);
        }

        [HttpPost("oauth")]
        public async Task<IActionResult> OAuthLogin([FromBody] OAuthRequest request)
        {
            var response = await _authService.HandleOAuthLogin(request.Email, request.FullName, request.Provider, request.AvatarUrl, request.Role);
            return Ok(response);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsers();
                // Map to DTOs to avoid IdentityUser serialization issues
                var result = users.Select(u => new
                {
                    id = u.Id,
                    fullName = u.FullName ?? "User",
                    userName = u.UserName ?? "user",
                    email = u.Email ?? "",
                    role = u.Role ?? "MEMBER",
                    isActive = u.IsActive,
                    avatarUrl = u.AvatarUrl ?? "",
                    provider = u.Provider ?? "LOCAL",
                    createdAt = u.CreatedAt
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ADMIN ERROR] GetAllUsers crashed: {ex}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string token)
        {
            var newToken = await _authService.RefreshToken(token);
            return Ok(new { token = newToken });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _authService.GetUserById(userId);
            return Ok(user);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var updatedUser = await _authService.UpdateProfile(userId, request);
            return Ok(updatedUser);
        }

        [Authorize]
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            request.UserId = userId;

            await _authService.ChangePassword(request);

            return Ok(new { message = "Password updated successfully" });
        }

        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            var users = await _authService.SearchUsers(query);
            return Ok(users);
        }

        [Authorize]
        [HttpDelete("deactivate")]
        public async Task<IActionResult> DeactivateAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _authService.DeactivateAccount(userId);

            return Ok(new { message = "Account deactivated" });
        }
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("users/{userId}/suspend")]
        public async Task<IActionResult> SuspendUser(string userId)
        {
            await _authService.SuspendUser(userId);
            return Ok(new { message = "User suspended successfully" });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("users/{userId}/reactivate")]
        public async Task<IActionResult> ReactivateUser(string userId)
        {
            await _authService.ReactivateUser(userId);
            return Ok(new { message = "User reactivated successfully" });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            await _authService.DeleteUser(userId);
            return Ok(new { message = "User deleted successfully" });
        }
    }
}