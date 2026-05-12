using System.Net;
using System.Text.Json;
using FlowBoard.Auth.Exceptions;

namespace FlowBoard.Auth.Middleware
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
                UserNotFoundException e          => (HttpStatusCode.NotFound,          e.Message),
                InvalidCredentialsException e    => (HttpStatusCode.Unauthorized,      e.Message),
                AccountDeactivatedException e    => (HttpStatusCode.Forbidden,         e.Message),
                EmailAlreadyExistsException e    => (HttpStatusCode.Conflict,          e.Message),
                UsernameAlreadyExistsException e => (HttpStatusCode.Conflict,          e.Message),
                InvalidTokenException e          => (HttpStatusCode.Unauthorized,      e.Message),
                ArgumentException e              => (HttpStatusCode.BadRequest,        e.Message),
                _                                => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var body = JsonSerializer.Serialize(new
            {
                statusCode = (int)statusCode,
                message,
                detail = exception.Message // Add actual error detail
            });

            return context.Response.WriteAsync(body);
        }
    }
}