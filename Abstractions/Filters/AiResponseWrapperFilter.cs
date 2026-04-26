using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json.Serialization;

namespace AI_genda_API.Abstractions.Filters;

/// <summary>
/// Enforces the strict {"status": "success/error", "data": {...}} JSON format
/// globally required by the Google ADK AI Agent.
/// </summary>
public class AiResponseWrapperFilter : IAsyncResultFilter
{
    public async System.Threading.Tasks.Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // Architectural Decision: We only apply this strict wrapping to paths 
        // starting with "/api/ai" to ensure the existing frontend contracts are NOT broken.
        if (!context.HttpContext.Request.Path.Value?.StartsWith("/api/ai", StringComparison.OrdinalIgnoreCase) ?? true)
        {
            await next();
            return;
        }

        var statusCode = context.HttpContext.Response.StatusCode;

        if (context.Result is ObjectResult objectResult)
        {
            statusCode = objectResult.StatusCode ?? statusCode;
            bool isSuccess = statusCode >= 200 && statusCode < 300;

            if (objectResult.Value is not AiResponse)
            {
                context.Result = new ObjectResult(new AiResponse
                {
                    Status = isSuccess ? "success" : "error",
                    Data = isSuccess ? objectResult.Value : null,
                    Message = !isSuccess ? objectResult.Value : null
                })
                {
                    StatusCode = statusCode
                };
            }
        }
        else if (context.Result is StatusCodeResult statusCodeResult)
        {
            bool isSuccess = statusCodeResult.StatusCode >= 200 && statusCodeResult.StatusCode < 300;
            context.Result = new ObjectResult(new AiResponse
            {
                Status = isSuccess ? "success" : "error",
                Data = isSuccess ? "Operation completed successfully" : null,
                Message = !isSuccess ? "An error occurred" : null
            })
            {
                StatusCode = statusCodeResult.StatusCode
            };
        }
        else if (context.Result is EmptyResult)
        {
            context.Result = new ObjectResult(new AiResponse
            {
                Status = "success",
                Data = "Operation completed successfully"
            })
            {
                StatusCode = 200
            };
        }

        await next();
    }
}

public class AiResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; set; }

    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Message { get; set; }
}