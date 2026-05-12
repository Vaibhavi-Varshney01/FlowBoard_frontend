using System.Text;
using System.Text.Json;

namespace FlowBoard.Web.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IHttpClientFactory factory, ILogger<NotificationService> logger)
        {
            _factory = factory;
            _logger  = logger;
        }

        private HttpClient Client => _factory.CreateClient("NotificationService");
        private StringContent Json(object obj) =>
            new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        private HttpRequestMessage WithAuth(HttpMethod method, string url, string token, object? body = null)
        {
            var req = new HttpRequestMessage(method, url);
            req.Headers.Add("Authorization", $"Bearer {token}");
            if (body != null) req.Content = Json(body);
            return req;
        }

        public async Task<List<object>> GetByRecipientAsync(int userId, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Get, $"api/notifications/recipient/{userId}", token));
                if (!res.IsSuccessStatusCode) return new();
                return JsonSerializer.Deserialize<List<object>>(await res.Content.ReadAsStringAsync()) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "GetByRecipient failed"); return new(); }
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Put, $"api/notifications/{notificationId}/read", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "MarkAsRead failed"); return false; }
        }

        public async Task<bool> MarkAllReadAsync(int userId, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Put, $"api/notifications/recipient/{userId}/readall", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "MarkAllRead failed"); return false; }
        }

        public async Task<bool> DeleteReadAsync(int userId, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Delete, $"api/notifications/recipient/{userId}/read", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "DeleteRead failed"); return false; }
        }

        public async Task<int> GetUnreadCountAsync(int userId, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Get, $"api/notifications/recipient/{userId}/unread-count", token));
                if (!res.IsSuccessStatusCode) return 0;
                return int.Parse(await res.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { _logger.LogError(ex, "GetUnreadCount failed"); return 0; }
        }

        public async Task<bool> DeleteAsync(int id, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Delete, $"api/notifications/{id}", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "Delete failed"); return false; }
        }

        public async Task<bool> SendBulkAsync(object model, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Post, "api/notifications/bulk", token, model))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "SendBulk failed"); return false; }
        }

        public async Task<List<object>> GetAllAsync(string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Get, "api/notifications", token));
                if (!res.IsSuccessStatusCode) return new();
                return JsonSerializer.Deserialize<List<object>>(await res.Content.ReadAsStringAsync()) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "GetAll failed"); return new(); }
        }
    }
}