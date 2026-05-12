using System.Text;
using System.Text.Json;
using FlowBoard.Web.DTOs;

namespace FlowBoard.Web.Services
{
    public class BoardService : IBoardService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<BoardService> _logger;

        public BoardService(IHttpClientFactory factory, ILogger<BoardService> logger)
        {
            _factory = factory;
            _logger  = logger;
        }

        private HttpClient Client => _factory.CreateClient("BoardService");
        private StringContent Json(object obj) =>
            new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        private HttpRequestMessage WithAuth(HttpMethod method, string url, string token, object? body = null)
        {
            var req = new HttpRequestMessage(method, url);
            req.Headers.Add("Authorization", $"Bearer {token}");
            if (body != null) req.Content = Json(body);
            return req;
        }

        public async Task<object?> GetBoardAsync(int id, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Get, $"api/boards/{id}", token));
                if (!res.IsSuccessStatusCode) return null;
                return JsonSerializer.Deserialize<object>(await res.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { _logger.LogError(ex, "GetBoard failed"); return null; }
        }

        public async Task<List<object>> GetByWorkspaceAsync(int workspaceId, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Get, $"api/boards/workspace/{workspaceId}", token));
                if (!res.IsSuccessStatusCode) return new();
                return JsonSerializer.Deserialize<List<object>>(await res.Content.ReadAsStringAsync()) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "GetByWorkspace failed"); return new(); }
        }

        public async Task<List<object>> GetByMemberAsync(int userId, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Get, $"api/boards/member/{userId}", token));
                if (!res.IsSuccessStatusCode) return new();
                return JsonSerializer.Deserialize<List<object>>(await res.Content.ReadAsStringAsync()) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "GetByMember failed"); return new(); }
        }

        public async Task<object?> CreateBoardAsync(CreateBoardDto dto, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Post, "api/boards", token, dto));
                if (!res.IsSuccessStatusCode) return null;
                return JsonSerializer.Deserialize<object>(await res.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { _logger.LogError(ex, "CreateBoard failed"); return null; }
        }

        public async Task<bool> UpdateBoardAsync(int id, object model, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Put, $"api/boards/{id}", token, model))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "UpdateBoard failed"); return false; }
        }

        public async Task<bool> CloseBoardAsync(int id, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Put, $"api/boards/{id}/close", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "CloseBoard failed"); return false; }
        }

        public async Task<bool> DeleteBoardAsync(int id, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Delete, $"api/boards/{id}", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "DeleteBoard failed"); return false; }
        }

        public async Task<bool> AddMemberAsync(int boardId, int userId, string role, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Post, $"api/boards/{boardId}/members/{userId}/{role}", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "AddMember failed"); return false; }
        }

        public async Task<bool> RemoveMemberAsync(int boardId, int userId, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Delete, $"api/boards/{boardId}/members/{userId}", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "RemoveMember failed"); return false; }
        }

        public async Task<List<object>> GetMembersAsync(int boardId, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Get, $"api/boards/{boardId}/members", token));
                if (!res.IsSuccessStatusCode) return new();
                return JsonSerializer.Deserialize<List<object>>(await res.Content.ReadAsStringAsync()) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "GetMembers failed"); return new(); }
        }
    }
}