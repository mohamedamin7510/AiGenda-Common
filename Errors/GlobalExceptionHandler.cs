using Microsoft.AspNetCore.Diagnostics;

namespace AI_genda_API.Errors;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger ): IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _Logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _Logger.LogError("An unhandled exception occurred: {message}",exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // If the request targets an AI proxy route, strictly comply with the AI JSON Contract
        if (httpContext.Request.Path.Value?.StartsWith("/api/ai", StringComparison.OrdinalIgnoreCase) == true)
        {
            await httpContext.Response.WriteAsJsonAsync(new 
            {
                status = "error",
                message = "An unexpected error occurred while processing the AI request."
            }, cancellationToken);

            return true;
        }

        // Standard response for all other API endpoints
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
