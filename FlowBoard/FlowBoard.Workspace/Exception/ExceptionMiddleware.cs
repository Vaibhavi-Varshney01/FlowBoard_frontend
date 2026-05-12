using System.Net;
using System.Text.Json;
using FlowBoard.Workspace.Exceptions;

namespace FlowBoard.Workspace.Middleware
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

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                WorkspaceNotFoundException e          => (HttpStatusCode.NotFound,           e.Message),
                MemberNotFoundException e             => (HttpStatusCode.NotFound,           e.Message),
                WorkspaceAlreadyExistsException e     => (HttpStatusCode.Conflict,           e.Message),
                MemberAlreadyExistsException e        => (HttpStatusCode.Conflict,           e.Message),
                CannotRemoveOwnerException e          => (HttpStatusCode.BadRequest,         e.Message),
                UnauthorizedWorkspaceAccessException e => (HttpStatusCode.Forbidden,         e.Message),
                UnauthorizedAccessException e         => (HttpStatusCode.Forbidden,          e.Message),
                KeyNotFoundException e               => (HttpStatusCode.NotFound,           e.Message),
                InvalidOperationException e          => (HttpStatusCode.BadRequest,         e.Message),
                _                                    => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var body = JsonSerializer.Serialize(new
            {
                statusCode = (int)statusCode,
                message
            });

            return context.Response.WriteAsync(body);
        }
    }
}