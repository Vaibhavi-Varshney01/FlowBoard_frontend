using System.Net;
using System.Text.Json;

namespace FlowBoard.List.Exceptions
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
                ListNotFoundException e      => (HttpStatusCode.NotFound, e.Message),
                ListArchivedException e      => (HttpStatusCode.Conflict, e.Message),
                ListNotArchivedException e   => (HttpStatusCode.Conflict, e.Message),
                InvalidListPositionException e => (HttpStatusCode.BadRequest, e.Message),
                ListAlreadyOnBoardException e => (HttpStatusCode.Conflict, e.Message),
                KeyNotFoundException e       => (HttpStatusCode.NotFound, e.Message),
                InvalidOperationException e  => (HttpStatusCode.Conflict, e.Message),
                ArgumentException e         => (HttpStatusCode.BadRequest, e.Message),
                UnauthorizedAccessException e => (HttpStatusCode.Forbidden, e.Message),
                _                           => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = (int)statusCode;

            var result = JsonSerializer.Serialize(new { message });
            return context.Response.WriteAsync(result);
        }
    }
}