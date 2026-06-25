using System.Net;
using System.Text.Json;
using StackOverflowLite.Application.Common.Exceptions;

namespace StackOverflowLite.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        object response;
        switch (exception)
        {
            case ValidationException validationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    message = "Validation failed",
                    errors = validationException.Errors
                };
                break;

            case NotFoundException notFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new { message = notFoundException.Message };
                break;

            case ForbiddenAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response = new { message = "You do not have permission to perform this action." };
                break;

            case BadRequestException badRequestException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new { message = badRequestException.Message };
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new { message = "An unexpected error occurred." };
                break;
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
