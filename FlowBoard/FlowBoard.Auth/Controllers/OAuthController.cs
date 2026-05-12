using FlowBoard.Auth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowBoard.Auth.Controllers
{
    [ApiController]
    [Route("api/auth/oauth")]
    public class OAuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public OAuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET /api/auth/oauth/google
        [HttpGet("google")]
        public IActionResult LoginWithGoogle()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(OAuthCallback), new { provider = "GOOGLE" })
            };
            return Challenge(properties, "Google");
        }

        // GET /api/auth/oauth/github
        [HttpGet("github")]
        public IActionResult LoginWithGitHub()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(OAuthCallback), new { provider = "GITHUB" })
            };
            return Challenge(properties, "GitHub");
        }

        // GET /api/auth/oauth/callback?provider=GOOGLE|GITHUB
        [HttpGet("callback")]
        public async Task<IActionResult> OAuthCallback([FromQuery] string provider)
        {
            var result = await HttpContext.AuthenticateAsync(provider);

            if (!result.Succeeded)
                return Unauthorized(new { message = "OAuth authentication failed" });

            var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
            var fullName = result.Principal?.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            if (string.IsNullOrEmpty(email))
                return BadRequest(new { message = "Email not provided by OAuth provider" });

            var avatarUrl = result.Principal?.FindFirstValue("urn:google:picture") ?? "";
            var authResponse = await _authService.HandleOAuthLogin(email, fullName, provider.ToUpper(), avatarUrl);

            return Ok(authResponse);
        }
    }
}