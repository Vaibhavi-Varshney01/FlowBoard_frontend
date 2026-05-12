using System.Text;
using System.Text.Json;
using FlowBoard.Web.DTOs;

namespace FlowBoard.Web.Services
{
    public class CardService : ICardService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<CardService> _logger;

        public CardService(IHttpClientFactory factory, ILogger<CardService> logger)
        {
            _factory = factory;
            _logger  = logger;
        }

        private HttpClient Client => _factory.CreateClient("CardService");
        private StringContent Json(object obj) =>
            new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        private HttpRequestMessage WithAuth(HttpMethod method, string url, string token, object? body = null)
        {
            var req = new HttpRequestMessage(method, url);
            req.Headers.Add("Authorization", $"Bearer {token}");
            if (body != null) req.Content = Json(body);
            return req;
        }

        private async Task<List<object>> GetListAsync(string url, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Get, url, token));
                if (!res.IsSuccessStatusCode) return new();
                return JsonSerializer.Deserialize<List<object>>(await res.Content.ReadAsStringAsync()) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "GetList failed for {Url}", url); return new(); }
        }

        public Task<object?>      GetCardAsync(int id, string token)           => GetObjAsync($"api/cards/{id}", token);
        public Task<List<object>> GetByListAsync(int listId, string token)     => GetListAsync($"api/cards/list/{listId}", token);
        public Task<List<object>> GetByBoardAsync(int boardId, string token)   => GetListAsync($"api/cards/board/{boardId}", token);
        public Task<List<object>> GetByAssigneeAsync(int uid, string token)    => GetListAsync($"api/cards/assignee/{uid}", token);
        public Task<List<object>> GetOverdueCardsAsync(string token)           => GetListAsync("api/cards/overdue", token);

        public async Task<object?> CreateCardAsync(CreateCardDto dto, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Post, "api/cards", token, dto));
                if (!res.IsSuccessStatusCode) return null;
                return JsonSerializer.Deserialize<object>(await res.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { _logger.LogError(ex, "CreateCard failed"); return null; }
        }

        public async Task<bool> UpdateCardAsync(int id, object model, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Put, $"api/cards/{id}", token, model))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "UpdateCard failed"); return false; }
        }

        public async Task<bool> MoveCardAsync(int id, int targetListId, int position, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Put, $"api/cards/{id}/move/{targetListId}/{position}", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "MoveCard failed"); return false; }
        }

        public async Task<bool> ReorderCardsAsync(int listId, List<int> cardIds, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Put, $"api/cards/reorder/{listId}", token, cardIds))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "ReorderCards failed"); return false; }
        }

        public async Task<bool> ArchiveCardAsync(int id, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Post, $"api/cards/{id}/archive", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "ArchiveCard failed"); return false; }
        }

        public async Task<bool> UnarchiveCardAsync(int id, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Post, $"api/cards/{id}/unarchive", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "UnarchiveCard failed"); return false; }
        }

        public async Task<bool> DeleteCardAsync(int id, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Delete, $"api/cards/{id}", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "DeleteCard failed"); return false; }
        }

        public async Task<bool> SetAssigneeAsync(int id, int assigneeId, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Put, $"api/cards/{id}/assignee/{assigneeId}", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "SetAssignee failed"); return false; }
        }

        public async Task<bool> SetPriorityAsync(int id, string priority, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Put, $"api/cards/{id}/priority/{priority}", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "SetPriority failed"); return false; }
        }

        private async Task<object?> GetObjAsync(string url, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Get, url, token));
                if (!res.IsSuccessStatusCode) return null;
                return JsonSerializer.Deserialize<object>(await res.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { _logger.LogError(ex, "GetObj failed for {Url}", url); return null; }
        }
    }
}