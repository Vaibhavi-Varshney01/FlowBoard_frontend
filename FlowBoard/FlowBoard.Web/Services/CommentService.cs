using System.Text;
using System.Text.Json;

namespace FlowBoard.Web.Services
{
    public class CommentService : ICommentService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<CommentService> _logger;

        public CommentService(IHttpClientFactory factory, ILogger<CommentService> logger)
        {
            _factory = factory;
            _logger  = logger;
        }

        private HttpClient Client => _factory.CreateClient("CommentService");
        private StringContent Json(object obj) =>
            new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        private HttpRequestMessage WithAuth(HttpMethod method, string url, string token, object? body = null)
        {
            var req = new HttpRequestMessage(method, url);
            req.Headers.Add("Authorization", $"Bearer {token}");
            if (body != null) req.Content = Json(body);
            return req;
        }

        public async Task<List<object>> GetByCardAsync(int cardId, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Get, $"api/comments/card/{cardId}", token));
                if (!res.IsSuccessStatusCode) return new();
                return JsonSerializer.Deserialize<List<object>>(await res.Content.ReadAsStringAsync()) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "GetByCard failed"); return new(); }
        }

        public async Task<object?> AddCommentAsync(int cardId, string content, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Post, "api/comments",
                    token, new { CardId = cardId, Content = content }));
                if (!res.IsSuccessStatusCode) return null;
                return JsonSerializer.Deserialize<object>(await res.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { _logger.LogError(ex, "AddComment failed"); return null; }
        }

        public async Task<bool> EditCommentAsync(int id, string content, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Put, $"api/comments/{id}", token, new { Content = content }))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "EditComment failed"); return false; }
        }

        public async Task<bool> DeleteCommentAsync(int id, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Delete, $"api/comments/{id}", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "DeleteComment failed"); return false; }
        }

        public async Task<List<object>> GetRepliesAsync(int commentId, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Get, $"api/comments/{commentId}/replies", token));
                if (!res.IsSuccessStatusCode) return new();
                return JsonSerializer.Deserialize<List<object>>(await res.Content.ReadAsStringAsync()) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "GetReplies failed"); return new(); }
        }

        public async Task<bool> AddAttachmentAsync(int cardId, IFormFile file, string token)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var stream  = file.OpenReadStream();
                content.Add(new StreamContent(stream), "file", file.FileName);
                var req = new HttpRequestMessage(HttpMethod.Post, $"api/attachments/card/{cardId}")
                    { Content = content };
                req.Headers.Add("Authorization", $"Bearer {token}");
                return (await Client.SendAsync(req)).IsSuccessStatusCode;
            }
            catch (Exception ex) { _logger.LogError(ex, "AddAttachment failed"); return false; }
        }

        public async Task<bool> DeleteAttachmentAsync(int id, string token)
        {
            try { return (await Client.SendAsync(WithAuth(HttpMethod.Delete, $"api/attachments/{id}", token))).IsSuccessStatusCode; }
            catch (Exception ex) { _logger.LogError(ex, "DeleteAttachment failed"); return false; }
        }

        public async Task<List<object>> GetAttachmentsByCardAsync(int cardId, string token)
        {
            try
            {
                var res = await Client.SendAsync(WithAuth(HttpMethod.Get, $"api/attachments/card/{cardId}", token));
                if (!res.IsSuccessStatusCode) return new();
                return JsonSerializer.Deserialize<List<object>>(await res.Content.ReadAsStringAsync()) ?? new();
            }
            catch (Exception ex) { _logger.LogError(ex, "GetAttachments failed"); return new(); }
        }
    }
}