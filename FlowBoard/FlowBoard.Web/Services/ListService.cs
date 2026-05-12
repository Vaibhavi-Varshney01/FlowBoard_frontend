using System.Text;
using System.Text.Json;

namespace FlowBoard.Web.Services
{
    public class ListService : IListService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<ListService> _logger;

        public ListService(IHttpClientFactory factory, ILogger<ListService> logger)
        {
            _factory = factory;
            _logger  = logger;
        }

        private HttpClient Client => _factory.CreateClient("BoardService");
        private StringContent Json(object obj) =>
            new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        public async Task<object?> CreateListAsync(object model, string token)
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Post, "api/lists") { Content = Json(model) };
                req.Headers.Add("Authorization", $"Bearer {token}");
                var res = await Client.SendAsync(req);
                if (!res.IsSuccessStatusCode) return null;
                return JsonSerializer.Deserialize<object>(await res.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { _logger.LogError(ex, "CreateList failed"); return null; }
        }

        public async Task<bool> ReorderListsAsync(int boardId, List<int> listIds, string token)
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Put, $"api/lists/reorder/{boardId}")
                    { Content = Json(listIds) };
                req.Headers.Add("Authorization", $"Bearer {token}");
                return (await Client.SendAsync(req)).IsSuccessStatusCode;
            }
            catch (Exception ex) { _logger.LogError(ex, "ReorderLists failed"); return false; }
        }
    }
}