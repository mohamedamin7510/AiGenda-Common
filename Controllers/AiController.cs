using AI_genda_API.Contracts.Ai;
using AI_genda_API.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AI_genda_API.Extenstions;

namespace AI_genda_API.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController(IAiService aiService, IAiPersistenceService aiPersistenceService) : ControllerBase
{
    private readonly IAiService _aiService = aiService;
    private readonly IAiPersistenceService _aiPersistenceService = aiPersistenceService;

    [HttpPost("chat")]
    public async System.Threading.Tasks.Task<IActionResult> Chat([FromBody] ChatRequest request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { success = false, data = (object?)null, error = "Unauthorized" });
        }

        var response = await _aiService.ChatAsync(userId, request, cancellationToken);
        return Ok(new { success = true, data = response, error = (object?)null });
    }

    [HttpPost("chat/stream")]
    public async System.Threading.Tasks.Task StreamChat([FromBody] ChatRequest request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            await Response.WriteAsJsonAsync(new { success = false, data = (object?)null, error = "Unauthorized" }, cancellationToken);
            return;
        }

        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";
        Response.ContentType = "text/event-stream";

        await foreach (var chunk in _aiService.StreamChatAsync(userId, request, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var payload = $"data: {chunk}\n\n";
            await Response.WriteAsync(payload, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { success = false, data = (object?)null, error = "Unauthorized" });
        }

        var response = await _aiService.GetStatusAsync(userId, cancellationToken);
        return Ok(new { success = true, data = response, error = (object?)null });
    }

    [HttpGet("welcome")]
    public async Task<IActionResult> Welcome(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { success = false, data = (object?)null, error = "Unauthorized" });
        }

        var response = await _aiService.GetWelcomeAsync(userId, cancellationToken);
        return Ok(new { success = true, data = response, error = (object?)null });
    }

    [HttpPost("tree")]
    public async System.Threading.Tasks.Task<IActionResult> PersistAgentTree([FromBody] AgentTreeRequest request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { success = false, data = (object?)null, error = "Unauthorized" });
        }

        var response = await _aiPersistenceService.PersistAgentTreeAsync(userId, request, cancellationToken);
        return Ok(new { success = true, data = response, error = (object?)null });
    }

    [HttpPost("clear")]
    public async System.Threading.Tasks.Task<IActionResult> Clear(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { success = false, data = (object?)null, error = "Unauthorized" });
        }

        var response = await _aiPersistenceService.FactoryResetAsync(userId, cancellationToken);
        return Ok(new { success = true, data = response, error = (object?)null });
    }
}
