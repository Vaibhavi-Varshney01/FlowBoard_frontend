using System.Text;
using System.Text.Json;

namespace FlowBoard.Web.Services
{
    public class LabelService : ILabelService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<LabelService> _logger;

        public LabelService(IHttpClientFactory factory, ILogger<LabelService> logger)
        {
            _factory = factory;
            _logger  = logger;
        }

        private HttpClient Client => _factory.CreateClient("LabelService");
        private StringContent Json(object obj) =>
            new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        private HttpRequestMessage WithAuth(HttpMethod method, string url, string token, object? body = null)
        {
            var req = new HttpRequestMessage(method, url);
            req.Headers.Add("Authorization", $"Bearer {token}");
            if (body != null) req.Content = Json(body);
            return req;
        }

        public async Task<bool> AddLabelToCardAsync(int cardId, int labelId, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Post, $"api/labels/card/{cardId}/{labelId}", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "AddLabelToCard failed"); return false; }
        }

        public async Task<bool> RemoveLabelFromCardAsync(int cardId, int labelId, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Delete, $"api/labels/card/{cardId}/{labelId}", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "RemoveLabelFromCard failed"); return false; }
        }

        public async Task<List<object>> GetLabelsForCardAsync(int cardId, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Get, $"api/labels/card/{cardId}", token));
                if (!res.IsSuccessStatusCode) return new();
                return JsonSerializer.Deserialize<List<object>>(await res.Content.ReadAsStringAsync()) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "GetLabelsForCard failed"); return new(); }
        }

        public async Task<object?> CreateChecklistAsync(object model, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Post, "api/checklists", token, model));
                if (!res.IsSuccessStatusCode) return null;
                return JsonSerializer.Deserialize<object>(await res.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { _logger.LogError(ex, "CreateChecklist failed"); return null; }
        }

        public async Task<bool> ToggleChecklistItemAsync(int itemId, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Put, $"api/checklists/items/{itemId}/toggle", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "ToggleChecklistItem failed"); return false; }
        }

        public async Task<double> GetChecklistProgressAsync(int checklistId, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Get, $"api/checklists/{checklistId}/progress", token));
                if (!res.IsSuccessStatusCode) return 0;
                return double.Parse(await res.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { _logger.LogError(ex, "GetChecklistProgress failed"); return 0; }
        }
    }
}