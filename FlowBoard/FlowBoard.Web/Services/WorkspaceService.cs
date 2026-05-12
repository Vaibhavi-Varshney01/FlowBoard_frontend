using System.Text;
using System.Text.Json;
using FlowBoard.Web.DTOs;

namespace FlowBoard.Web.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<WorkspaceService> _logger;

        public WorkspaceService(IHttpClientFactory factory, ILogger<WorkspaceService> logger)
        {
            _factory = factory;
            _logger  = logger;
        }

        private HttpClient Client => _factory.CreateClient("WorkspaceService");

        private StringContent Json(object obj) =>
            new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        private HttpRequestMessage AuthGet(string url, string token)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("Authorization", $"Bearer {token}");
            return req;
        }

        public async Task<object?> GetWorkspaceAsync(int id, string token)
        {
            try
            {
                var res = await Client.SendAsync(AuthGet($"api/workspaces/{id}", token));
                if (!res.IsSuccessStatusCode) return null;
                return JsonSerializer.Deserialize<object>(await res.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { _logger.LogError(ex, "GetWorkspace failed"); return null; }
        }

        public async Task<List<object>> GetByOwnerAsync(int ownerId, string token)
        {
            try
            {
                var res = await Client.SendAsync(AuthGet($"api/workspaces/owner/{ownerId}", token));
                if (!res.IsSuccessStatusCode) return new();
                return JsonSerializer.Deserialize<List<object>>(await res.Content.ReadAsStringAsync()) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "GetByOwner failed"); return new(); }
        }

        public async Task<List<object>> GetByMemberAsync(int userId, string token)
        {
            try
            {
                var res = await Client.SendAsync(AuthGet($"api/workspaces/member/{userId}", token));
                if (!res.IsSuccessStatusCode) return new();
                return JsonSerializer.Deserialize<List<object>>(await res.Content.ReadAsStringAsync()) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "GetByMember failed"); return new(); }
        }

        public async Task<object?> CreateWorkspaceAsync(CreateWorkspaceDto dto, string token)
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Post, "api/workspaces") { Content = Json(dto) };
                req.Headers.Add("Authorization", $"Bearer {token}");
                var res = await Client.SendAsync(req);
                if (!res.IsSuccessStatusCode) return null;
                return JsonSerializer.Deserialize<object>(await res.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { _logger.LogError(ex, "CreateWorkspace failed"); return null; }
        }

        public async Task<bool> UpdateWorkspaceAsync(int id, object model, string token)
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Put, $"api/workspaces/{id}") { Content = Json(model) };
                req.Headers.Add("Authorization", $"Bearer {token}");
                return (await Client.SendAsync(req)).IsSuccessStatusCode;
            }
            catch (Exception ex) { _logger.LogError(ex, "UpdateWorkspace failed"); return false; }
        }

        public async Task<bool> DeleteWorkspaceAsync(int id, string token)
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Delete, $"api/workspaces/{id}");
                req.Headers.Add("Authorization", $"Bearer {token}");
                return (await Client.SendAsync(req)).IsSuccessStatusCode;
            }
            catch (Exception ex) { _logger.LogError(ex, "DeleteWorkspace failed"); return false; }
        }

        public async Task<bool> AddMemberAsync(int workspaceId, int userId, string role, string token)
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Post,
                    $"api/workspaces/{workspaceId}/members/{userId}/{role}");
                req.Headers.Add("Authorization", $"Bearer {token}");
                return (await Client.SendAsync(req)).IsSuccessStatusCode;
            }
            catch (Exception ex) { _logger.LogError(ex, "AddMember failed"); return false; }
        }

        public async Task<bool> RemoveMemberAsync(int workspaceId, int userId, string token)
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Delete,
                    $"api/workspaces/{workspaceId}/members/{userId}");
                req.Headers.Add("Authorization", $"Bearer {token}");
                return (await Client.SendAsync(req)).IsSuccessStatusCode;
            }
            catch (Exception ex) { _logger.LogError(ex, "RemoveMember failed"); return false; }
        }

        public async Task<List<object>> GetMembersAsync(int workspaceId, string token)
        {
            try
            {
                var res = await Client.SendAsync(AuthGet($"api/workspaces/{workspaceId}/members", token));
                if (!res.IsSuccessStatusCode) return new();
                return JsonSerializer.Deserialize<List<object>>(await res.Content.ReadAsStringAsync()) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "GetMembers failed"); return new(); }
        }
    }
}