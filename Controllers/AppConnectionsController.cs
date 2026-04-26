using AI_genda_API.Abstractions.Const;
using AI_genda_API.Abstractions.Filters;
using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Contracts.AppConnections;
using AI_genda_API.Services.AppConnectionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace AI_genda_API.Controllers;

[Route("api/users/current/app-connections")]
[ApiController]
[Authorize]
public class AppConnectionsController(IAppConnectionService appConnectionService) : ControllerBase
{
    private readonly IAppConnectionService _AppConnectionService = appConnectionService;

    /// <summary>
    /// Gets all connected apps for the current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConnections(
        CancellationToken cancellationToken = default)
    {
        var result = await _AppConnectionService.GetConnectionsAsync(
            User.GetUserId()!,
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Gets details of a specific app connection
    /// </summary>
    [HttpGet("{connectionId}")]
    public async Task<IActionResult> GetConnection(
        [FromRoute] string connectionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _AppConnectionService.GetConnectionAsync(
            connectionId,
            User.GetUserId()!,
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Starts OAuth flow to connect a new app
    /// </summary>
    [HttpPost("authorize/{provider}")]
    public async Task<IActionResult> GetAuthorizationUrl(
        [FromRoute] string provider,
        [FromQuery] int? workspaceId = null,
        [FromServices] Microsoft.Extensions.Caching.Distributed.IDistributedCache cache = null!,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<AppProvider>(provider, true, out var appProvider))
            return BadRequest("Invalid provider");

        // Generate secure random state
        var state = Guid.NewGuid().ToString("N");
        var userId = User.GetUserId()!;
        var stateValue = workspaceId.HasValue ? $"{workspaceId.Value}|{userId}|{Guid.NewGuid()}" : $"0|{userId}|{Guid.NewGuid()}";

        // Cache the state for CSRF validation and passing workspace ID to the callback
        await cache.SetStringAsync(
            $"oauth_state:{state}", 
            stateValue, 
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            }, 
            cancellationToken);

        var authUrl = _AppConnectionService.GetAuthorizationUrl(appProvider, state);

        return Ok(new { authorizationUrl = authUrl });
    }

    /// <summary>
    /// Updates app connection settings
    /// </summary>
    [HttpPut("{connectionId}")]
    public async Task<IActionResult> UpdateConnection(
        [FromRoute] string connectionId,
        [FromBody] AppConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _AppConnectionService.UpdateConnectionAsync(
            connectionId,
            User.GetUserId()!,
            request,
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Disconnects an app and removes all associated data
    /// </summary>
    [HttpDelete("{connectionId}")]
    public async Task<IActionResult> DisconnectApp(
        [FromRoute] string connectionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _AppConnectionService.DisconnectAsync(
            connectionId,
            User.GetUserId()!,
            cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    /// <summary>
    /// Manually trigger a sync for a connection
    /// </summary>
    [HttpPost("{connectionId}/sync")]
    public async Task<IActionResult> SyncNow(
        [FromRoute] string connectionId,
        [FromBody] ManualSyncRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _AppConnectionService.SyncNowAsync(
            connectionId,
            User.GetUserId()!,
            request.ForceFullSync,
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Gets the sync status of a connection
    /// </summary>
    [HttpGet("{connectionId}/sync-status")]
    public async Task<IActionResult> GetSyncStatus(
        [FromRoute] string connectionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _AppConnectionService.GetSyncStatusAsync(
            connectionId,
            User.GetUserId()!,
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
