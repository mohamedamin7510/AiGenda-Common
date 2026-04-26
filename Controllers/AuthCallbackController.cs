using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Contracts.AppConnections;
using AI_genda_API.Services.AppConnectionService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace AI_genda_API.Controllers;

/// <summary>
/// Handles OAuth callbacks from external providers (Google, GitHub, etc.)
/// Exchanges authorization code for access tokens and creates app connections
/// </summary>
[Route("api/auth/callback")]
[ApiController]
public class AuthCallbackController(
    IAppConnectionService appConnectionService,
    IDistributedCache cache,
    IConfiguration configuration,
    ILogger<AuthCallbackController> logger) : ControllerBase
{
    private readonly IAppConnectionService _appConnectionService = appConnectionService;
    private readonly IDistributedCache _cache = cache;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<AuthCallbackController> _logger = logger;

    /// <summary>
    /// Handles OAuth callbacks from all providers
    /// 
    /// Example: /api/auth/callback/google?code=4/xxxxx&state=abc123
    /// </summary>
    [HttpGet("{provider}")]
    public async Task<IActionResult> HandleCallback(
        [FromRoute] string provider,
        [FromQuery] string? code = null,
        [FromQuery] string? state = null,
        [FromQuery] int? workspaceId = null,
        [FromQuery] string? error = null,
        [FromQuery] string? error_description = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "OAuth callback received. Provider: {Provider}, HasCode: {HasCode}, HasError: {HasError}",
                provider, !string.IsNullOrEmpty(code), !string.IsNullOrEmpty(error));

            // === STEP 1: Check for OAuth errors from provider ===
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning(
                    "OAuth provider returned error. Provider: {Provider}, Error: {Error}, Description: {Description}",
                    provider, error, error_description);

                var errorUrl = BuildErrorUrl("oauth_denied", $"{error}: {error_description}");
                return Redirect(errorUrl);
            }

            // === STEP 2: Validate required parameters ===
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogWarning("OAuth callback missing code parameter. Provider: {Provider}", provider);
                return Redirect(BuildErrorUrl("missing_code", "Authorization code not provided"));
            }

            if (string.IsNullOrEmpty(state))
            {
                _logger.LogWarning("OAuth callback missing state parameter. Provider: {Provider}", provider);
                return Redirect(BuildErrorUrl("missing_state", "State parameter not provided"));
            }

            // === STEP 3: Validate state parameter (CSRF protection) ===
            var cachedState = await _cache.GetStringAsync($"oauth_state:{state}", cancellationToken);
            
            if (string.IsNullOrEmpty(cachedState))
            {
                _logger.LogWarning(
                    "OAuth state validation failed. Provider: {Provider}, State: {State}",
                    provider, state);

                return Redirect(BuildErrorUrl("invalid_state", "State parameter validation failed - possible CSRF attack"));
            }

            // Remove state from cache (can only be used once)
            await _cache.RemoveAsync($"oauth_state:{state}", cancellationToken);

            // === STEP 4: Parse state to get workspace ID and User ID ===
            // State format: "{workspaceId}|{userId}|{random}"
            var stateparts = cachedState.Split('|');
            if (!int.TryParse(stateparts[0], out var parsedWorkspaceId))
            {
                _logger.LogWarning("Failed to parse workspace ID from state. State: {State}", cachedState);
                return Redirect(BuildErrorUrl("invalid_state", "Failed to parse workspace information"));
            }

            // === STEP 5: Get user ID from state ===
            var userId = stateparts.Length > 1 ? stateparts[1] : null;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not authenticated. Cannot create app connection.");
                return Redirect(BuildErrorUrl("not_authenticated", "User not authenticated"));
            }

            // === STEP 6: Parse provider ===
            if (!Enum.TryParse<AppProvider>(provider, true, out var appProvider))
            {
                _logger.LogWarning("Unknown provider in callback. Provider: {Provider}", provider);
                return Redirect(BuildErrorUrl("unknown_provider", $"Unknown provider: {provider}"));
            }

            // === STEP 7: Exchange code for tokens ===
            _logger.LogInformation("Exchanging authorization code for tokens. Provider: {Provider}, User: {UserId}",
                provider, userId);

            var result = await _appConnectionService.CreateConnectionAsync(
                parsedWorkspaceId,
                userId,
                appProvider,
                code,
                BuildRedirectUri(provider),
                cancellationToken: cancellationToken);

            // === STEP 8: Handle result ===
            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "App connection created successfully. Provider: {Provider}, ConnectionId: {ConnectionId}, User: {UserId}",
                    provider, result.Value.Id, userId);

                // Redirect to frontend with success
                var successUrl = $"{GetFrontendBaseUrl()}/connected-apps?status=success&provider={provider}&connectionId={result.Value.Id}";
                return Redirect(successUrl);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to create app connection. Provider: {Provider}, Error: {Error}",
                    provider, result.Error?.Code);

                var errorUrl = BuildErrorUrl("connection_failed", result.Error?.Descrption ?? "Failed to create connection");
                return Redirect(errorUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in OAuth callback handler. Provider: {Provider}", provider);
            return Redirect(BuildErrorUrl("server_error", "An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Helper: Build error redirect URL
    /// Redirects to frontend with error details
    /// </summary>
    private string BuildErrorUrl(string errorCode, string errorMessage)
    {
        var baseUrl = GetFrontendBaseUrl();
        var encodedMessage = Uri.EscapeDataString(errorMessage);
        return $"{baseUrl}/connected-apps?status=error&error={errorCode}&message={encodedMessage}";
    }

    /// <summary>
    /// Helper: Get frontend base URL from configuration or environment
    /// </summary>
    private string GetFrontendBaseUrl()
    {
        // Try to get from configuration, fallback to development default
        var frontendUrl = _configuration["Frontend:BaseUrl"];
        if (!string.IsNullOrEmpty(frontendUrl))
            return frontendUrl;

        // Development fallback
        return "http://localhost:5173";
    }

    /// <summary>
    /// Helper: Build redirect URI for OAuth provider
    /// Must match exactly what's registered with the provider
    /// </summary>
    private string BuildRedirectUri(string provider)
    {
        var baseUrl = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
        return $"{baseUrl}/api/auth/callback/{provider.ToLower()}";
    }
}
