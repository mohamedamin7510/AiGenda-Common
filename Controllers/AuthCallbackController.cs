using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Contracts.AppConnections;
using AI_genda_API.Services.AppConnectionService;
using Microsoft.AspNetCore.Mvc;

namespace AI_genda_API.Controllers;

/// <summary>
/// Handles OAuth callbacks from external providers (Google, GitHub, etc.)
/// Exchanges authorization code for access tokens and creates app connections
/// </summary>
[Route("api/auth/callback")]
[ApiController]
public class AuthCallbackController(
    IAppConnectionService appConnectionService,
    IConfiguration configuration,
    ILogger<AuthCallbackController> logger) : ControllerBase
{
    private readonly IAppConnectionService _appConnectionService = appConnectionService;
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
        [FromQuery] string? error = null,
        [FromQuery] string? error_description = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "OAuth callback received. Provider: {Provider}, HasCode: {HasCode}, HasError: {HasError}",
                provider, !string.IsNullOrEmpty(code), !string.IsNullOrEmpty(error));

            // Validate any OAuth error responses returned by the provider.
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning(
                    "OAuth provider returned error. Provider: {Provider}, Error: {Error}, Description: {Description}",
                    provider, error, error_description);

                var errorUrl = BuildErrorUrl("oauth_denied", $"{error}: {error_description}");
                return Redirect(errorUrl);
            }

            // Validate required callback parameters.
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

            var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            // Validate the provider name from the route parameter.
            if (!Enum.TryParse<AppProvider>(provider, true, out var appProvider))
            {
                _logger.LogWarning("Unknown provider in callback. Provider: {Provider}", provider);
                return env.IsDevelopment() 
                    ? Ok(new { error = "unknown_provider", description = $"Unknown provider: {provider}" })
                    : Redirect(BuildErrorUrl("unknown_provider", $"Unknown provider: {provider}"));
            }

            // Validate and decrypt the OAuth state using the shared IDataProtection purpose in the service.
            _logger.LogInformation("Validating OAuth state and exchanging code. Provider: {Provider}", provider);
            var result = await _appConnectionService.HandleOAuthCallbackAsync(code, state, BuildRedirectUri(provider));

            // Handle the connection result.
            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "App connection created successfully. Provider: {Provider}, ConnectionId: {ConnectionId}",
                    provider, result.Value.Id);

                if (env.IsDevelopment())
                {
                    // Return JSON response for clients that expect API output instead of redirects.
                    return Ok(new 
                    { 
                        Message = "Connection successful", 
                        ConnectionId = result.Value.Id, 
                        Provider = provider,
                        Status = "Connected"
                    });
                }

                // Redirect to the frontend with a success status.
                var successUrl = $"{GetFrontendBaseUrl()}/connected-apps?status=success&provider={provider}&connectionId={result.Value.Id}";
                return Redirect(successUrl);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to create app connection. Provider: {Provider}, Error: {Error}",
                    provider, result.Error?.Code);

                if (env.IsDevelopment())
                {
                    return Ok(new { error = result.Error?.Code, description = result.Error?.Descrption });
                }

                var errorUrl = BuildErrorUrl("connection_failed", result.Error?.Descrption ?? "Failed to create connection");
                return Redirect(errorUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in OAuth callback handler. Provider: {Provider}", provider);

            var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            if (env.IsDevelopment())
            {
                 return StatusCode(500, new { error = "server_error", description = ex.Message, stackTrace = ex.StackTrace });
            }

            return Redirect(BuildErrorUrl("server_error", "An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Helper: Build error redirect URL for the frontend.
    /// </summary>
    private string BuildErrorUrl(string errorCode, string errorMessage)
    {
        var baseUrl = GetFrontendBaseUrl();
        var encodedMessage = Uri.EscapeDataString(errorMessage);
        return $"{baseUrl}/connected-apps?status=error&error={errorCode}&message={encodedMessage}";
    }

    /// <summary>
    /// Helper: Resolve the frontend base URL from configuration.
    /// </summary>
    private string GetFrontendBaseUrl()
    {
        // Try to get from configuration, fallback to the local development default.
        var frontendUrl = _configuration["Frontend:BaseUrl"];
        if (!string.IsNullOrEmpty(frontendUrl))
            return frontendUrl;

        // Development fallback.
        return "http://localhost:5173";
    }

    /// <summary>
    /// Helper: Build redirect URI for the OAuth provider.
    /// Must match what is registered with the provider.
    /// </summary>
    private string BuildRedirectUri(string provider)
    {
        var baseUrl = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
        return $"{baseUrl}/api/auth/callback/{provider.ToLower()}";
    }
}
