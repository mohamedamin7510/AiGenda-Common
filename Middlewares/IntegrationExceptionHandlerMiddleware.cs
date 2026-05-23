using System.Net;
using System.Text.Json;
using Task = System.Threading.Tasks.Task;
using AI_genda_API.Contracts.AppConnections;
using AI_genda_API.Exceptions;

namespace AI_genda_API.Middlewares;

public class IntegrationExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IntegrationExceptionHandlerMiddleware> _logger;

    public IntegrationExceptionHandlerMiddleware(RequestDelegate next, ILogger<IntegrationExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            // Only handle exceptions intended for integration endpoints
            if (httpContext.Request.Path.StartsWithSegments("/integrations"))
            {
                await HandleIntegrationExceptionAsync(httpContext, ex);
            }
            else
            {
                throw; // Rethrow to native API exception handler if it's not an integration endpoint
            }
        }
    }

    private async Task HandleIntegrationExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An integration error occurred.");

        context.Response.ContentType = "application/json";

        context.Response.StatusCode = exception switch
        {
            IntegrationUnauthorizedException => (int)HttpStatusCode.Unauthorized,     // 401
            IntegrationMissingException => (int)HttpStatusCode.Forbidden,          // 403
            IntegrationNotFoundException => (int)HttpStatusCode.NotFound,         // 404
            IntegrationRateLimitException => (int)HttpStatusCode.TooManyRequests, // 429
            _ => (int)HttpStatusCode.InternalServerError                          // 500
        };

        var response = IntegrationResponse<object>.Error(exception.Message);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(json);
    }
}
