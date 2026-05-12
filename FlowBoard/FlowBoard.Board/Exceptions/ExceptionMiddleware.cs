using System.Net;
using System.Text.Json;

namespace FlowBoard.Board.Exceptions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (statusCode, message) = ex switch
            {
                KeyNotFoundException e       => (HttpStatusCode.NotFound, e.Message),
                InvalidOperationException e  => (HttpStatusCode.BadRequest, e.Message),
                UnauthorizedAccessException e => (HttpStatusCode.Forbidden, e.Message),
                ArgumentException e          => (HttpStatusCode.BadRequest, e.Message),
                _                           => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = (int)statusCode;

            var result = JsonSerializer.Serialize(new { message });
            return context.Response.WriteAsync(result);
        }
    }
}