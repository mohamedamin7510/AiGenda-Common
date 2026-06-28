using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Contracts.AppConnections;
using AI_genda_API.Services.AppConnectionService;
using Microsoft.AspNetCore.Mvc;

namespace AI_genda_API.Controllers;

/// <summary>
/// Handles OAuth callbacks from external providers (Google, GitHub, etc.)
/// Exchanges authorization code for access tokens and creates app connections
/// Supports both browser-based (redirect) and API-based (JSON) clients
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
    /// Supports both mobile (Custom Tabs Redirect) and web browser (web redirect/JSON) flows
    /// </summary>
    [HttpGet("{provider}")]
    public async Task<IActionResult> HandleCallback(
        [FromRoute] string provider,
        [FromQuery] string? code = null,
        [FromQuery] string? state = null,
        [FromQuery] string? error = null,
        [FromQuery] string? error_description = null,
        [FromQuery] bool? apiClient = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool isMobile = false;
            bool returnJson = apiClient == true; // Explicit API client flag for web apps

            // التحقق من بادئة الموبايل المخزنة داخل الـ state
            if (!string.IsNullOrEmpty(state) && state.StartsWith("mobile_"))
            {
                isMobile = true;
                state = state.Substring("mobile_".Length); // تنظيف الـ state لإكمال التحقق الأصلي
            }

            _logger.LogInformation(
                "OAuth callback received. Provider: {Provider}, IsMobile: {IsMobile}, ReturnJson: {ReturnJson}, HasError: {HasError}",
                provider, isMobile, returnJson, !string.IsNullOrEmpty(error));

            // Handle OAuth provider errors
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("OAuth provider returned error: {Error}: {Description}", error, error_description);

                if (isMobile)
                {
                    return Redirect($"aigenda://error?status=error&error={error}&message={Uri.EscapeDataString(error_description ?? "OAuth denied")}");
                }
                if (returnJson)
                {
                    return BadRequest(new OAuthCallbackErrorResponse(
                        errorCode: error,
                        errorMessage: error_description ?? "OAuth provider rejected the request",
                        details: $"Provider: {provider}"
                    ));
                }

                return Redirect(BuildErrorUrl("oauth_denied", $"{error}: {error_description}"));
            }

            // Validate required parameters
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                _logger.LogWarning("Missing required OAuth parameters. Code: {HasCode}, State: {HasState}",
                    !string.IsNullOrEmpty(code), !string.IsNullOrEmpty(state));

                if (isMobile)
                {
                    return Redirect("aigenda://error?status=error&error=bad_request&message=Parameters+missing");
                }
                if (returnJson)
                {
                    return BadRequest(new OAuthCallbackErrorResponse(
                        errorCode: "bad_request",
                        errorMessage: "Required OAuth parameters are missing",
                        details: "state and code are required"
                    ));
                }

                return Redirect(BuildErrorUrl("bad_request", "Required parameters are missing"));
            }

            // Validate provider
            if (!Enum.TryParse<AppProvider>(provider, true, out var appProvider))
            {
                _logger.LogWarning("Unknown OAuth provider requested: {Provider}", provider);

                if (isMobile)
                {
                    return Redirect($"aigenda://error?status=error&error=unknown_provider&message=Unknown+provider+{provider}");
                }
                if (returnJson)
                {
                    return BadRequest(new OAuthCallbackErrorResponse(
                        errorCode: "unknown_provider",
                        errorMessage: $"Unknown OAuth provider: {provider}"
                    ));
                }

                return Redirect(BuildErrorUrl("unknown_provider", $"Unknown provider: {provider}"));
            }

            // Process OAuth callback
            var result = await _appConnectionService.HandleOAuthCallbackAsync(
                code, state, BuildRedirectUri(provider));

            if (result.IsSuccess)
            {
                _logger.LogInformation("OAuth connection successful. Provider: {Provider}, ConnectionId: {ConnectionId}",
                    provider, result.Value?.Id);

                // فلو الموبايل الحتمي: ريدايركت صريح لإجبار الـ Custom Tabs على الإغلاق التلقائي
                if (isMobile)
                {
                    return Redirect($"aigenda://success?status=success&connectionId={result.Value!.Id}");
                }

                if (returnJson)
                {
                    return Ok(new OAuthCallbackSuccessResponse(
                        Status: "success",
                        Provider: provider,
                        ConnectionId: result.Value!.Id,
                        ExternalAccountId: result.Value.ExternalAccountId ?? "unknown",
                        Message: "Connection successfully established"
                    ));
                }

                var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                if (env.IsDevelopment())
                {
                    return Ok(new OAuthCallbackSuccessResponse(
                        Status: "success",
                        Provider: provider,
                        ConnectionId: result.Value!.Id,
                        ExternalAccountId: result.Value.ExternalAccountId ?? "unknown"
                    ));
                }

                return Redirect($"{GetFrontendBaseUrl()}/connected-apps?status=success&provider={provider}&connectionId={result.Value.Id}");
            }
            else
            {
                _logger.LogError("OAuth connection failed. Provider: {Provider}, Error: {ErrorCode}, Description: {ErrorDescription}",
                    provider, result.Error?.Code, result.Error?.Descrption);

                if (isMobile)
                {
                    return Redirect($"aigenda://error?status=error&error={result.Error?.Code ?? "connection_failed"}&message={Uri.EscapeDataString(result.Error?.Descrption ?? "Failed to create app connection")}");
                }

                if (returnJson)
                {
                    return BadRequest(new OAuthCallbackErrorResponse(
                        errorCode: result.Error?.Code ?? "connection_failed",
                        errorMessage: result.Error?.Descrption ?? "Failed to create app connection",
                        details: provider
                    ));
                }

                var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                if (env.IsDevelopment())
                {
                    return BadRequest(new OAuthCallbackErrorResponse(
                        errorCode: result.Error?.Code ?? "connection_failed",
                        errorMessage: result.Error?.Descrption ?? "Failed to create app connection"
                    ));
                }

                return Redirect(BuildErrorUrl(
                    result.Error?.Code ?? "connection_failed",
                    result.Error?.Descrption ?? "Failed to create connection"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in OAuth callback handler for provider: {Provider}", provider);

            bool isMobileException = HttpContext.Request.Query.TryGetValue("state", out var stateValue) &&
                                     stateValue.ToString().StartsWith("mobile_");

            if (isMobileException)
            {
                return Redirect($"aigenda://error?status=error&error=server_error&message={Uri.EscapeDataString(ex.Message)}");
            }

            if (apiClient == true)
            {
                return StatusCode(500, new OAuthCallbackErrorResponse(
                    errorCode: "server_error",
                    errorMessage: "An unexpected error occurred during OAuth callback processing",
                    details: ex.Message
                ));
            }

            var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            return env.IsDevelopment()
                ? StatusCode(500, new OAuthCallbackErrorResponse(
                    errorCode: "server_error",
                    errorMessage: "An unexpected error occurred",
                    details: ex.Message))
                : Redirect(BuildErrorUrl("server_error", "An unexpected error occurred"));
        }
    }

    private string BuildErrorUrl(string errorCode, string errorMessage)
    {
        var baseUrl = GetFrontendBaseUrl();
        var encodedMessage = Uri.EscapeDataString(errorMessage);
        return $"{baseUrl}/connected-apps?status=error&error={errorCode}&message={encodedMessage}";
    }

    private string GetFrontendBaseUrl()
    {
        var frontendUrl = _configuration["Frontend:BaseUrl"];
        if (!string.IsNullOrEmpty(frontendUrl))
            return frontendUrl;

        return "http://localhost:5173";
    }

    private string BuildRedirectUri(string provider)
    {
        var redirectUri = _configuration[$"OAuth:{provider}:RedirectUri"];
        if (!string.IsNullOrEmpty(redirectUri))
        {
            return redirectUri;
        }

        return $"https://AigendaTest.runasp.net/api/auth/callback/{provider.ToLower()}";
    }
}