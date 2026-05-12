using System.Text;
using System.Text.Json;
using FlowBoard.Web.DTOs;

namespace FlowBoard.Web.Services
{
    // Calls FlowBoard.Auth microservice via IHttpClientFactory
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IHttpClientFactory factory, ILogger<AuthService> logger)
        {
            _factory = factory;
            _logger  = logger;
        }

        private HttpClient Client => _factory.CreateClient("AuthService");

        private StringContent Json(object obj) =>
            new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        public async Task<string?> LoginAsync(LoginDto dto)
        {
            try
            {
                var res = await Client.PostAsync("api/auth/login", Json(dto));
                if (!res.IsSuccessStatusCode) return null;
                var body = await res.Content.ReadAsStringAsync();
                var doc  = JsonDocument.Parse(body);
                return doc.RootElement.GetProperty("token").GetString();
            }
            catch (Exception ex) { _logger.LogError(ex, "Login failed"); return null; }
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            try
            {
                var res = await Client.PostAsync("api/auth/register", Json(dto));
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex) { _logger.LogError(ex, "Register failed"); return false; }
        }

        public Task LogoutAsync(HttpContext context)
        {
            context.Session.Clear();
            return Task.CompletedTask;
        }

        public async Task<object?> GetProfileAsync(string token)
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Get, "api/auth/profile");
                req.Headers.Add("Authorization", $"Bearer {token}");
                var res = await Client.SendAsync(req);
                if (!res.IsSuccessStatusCode) return null;
                return JsonSerializer.Deserialize<object>(await res.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { _logger.LogError(ex, "GetProfile failed"); return null; }
        }

        public async Task<bool> UpdateProfileAsync(string token, object model)
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Put, "api/auth/profile") { Content = Json(model) };
                req.Headers.Add("Authorization", $"Bearer {token}");
                var res = await Client.SendAsync(req);
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex) { _logger.LogError(ex, "UpdateProfile failed"); return false; }
        }

        public async Task<bool> ChangePasswordAsync(string token, int userId, string newPassword)
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Put, "api/auth/password")
                {
                    Content = Json(new { UserId = userId, NewPassword = newPassword })
                };
                req.Headers.Add("Authorization", $"Bearer {token}");
                var res = await Client.SendAsync(req);
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex) { _logger.LogError(ex, "ChangePassword failed"); return false; }
        }

        public async Task<List<object>> SearchUsersAsync(string query)
        {
            try
            {
                var res = await Client.GetAsync($"api/auth/search?q={query}");
                if (!res.IsSuccessStatusCode) return new();
                return JsonSerializer.Deserialize<List<object>>(await res.Content.ReadAsStringAsync()) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "SearchUsers failed"); return new(); }
        }

        public async Task<bool> DeactivateAccountAsync(string token, int userId)
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Delete, $"api/auth/deactivate/{userId}");
                req.Headers.Add("Authorization", $"Bearer {token}");
                var res = await Client.SendAsync(req);
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex) { _logger.LogError(ex, "DeactivateAccount failed"); return false; }
        }
    }
}