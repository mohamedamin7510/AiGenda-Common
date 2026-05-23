using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Contracts.Integrations.GitHub;
using AI_genda_API.Contracts.Integrations.Gmail;
using AI_genda_API.Contracts.Integrations.GoogleCalendar;
using AI_genda_API.Exceptions;
using AI_genda_API.Services.AppConnectionService;
using AI_genda_API.Services.GitHubIntegrationService;
using AI_genda_API.Services.GmailIntegrationService;
using AI_genda_API.Services.GoogleCalendarIntegrationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AI_genda_API.Controllers;

[ApiController]
[Route("integrations/v1/")]
public class IntegrationsController : IntegrationController
{
    private readonly IAppConnectionService _appConnectionService;
    private readonly IGitHubIntegrationService _gitHubIntegrationService;
    private readonly IGmailIntegrationService _gmailIntegrationService;
    private readonly IGoogleCalendarIntegrationService _googleCalendarIntegrationService;

    public IntegrationsController(
        IAppConnectionService appConnectionService, 
        IGitHubIntegrationService gitHubIntegrationService,
        IGmailIntegrationService gmailIntegrationService,
        IGoogleCalendarIntegrationService googleCalendarIntegrationService)
    {
        _appConnectionService = appConnectionService;
        _gitHubIntegrationService = gitHubIntegrationService;
        _gmailIntegrationService = gmailIntegrationService;
        _googleCalendarIntegrationService = googleCalendarIntegrationService;
    }

    [Authorize]
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken = default)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(authenticatedUserId))
            throw new IntegrationUnauthorizedException("Requested user does not match the authenticated access token.");

        var statusResult = await _appConnectionService.GetIntegrationsStatusAsync(authenticatedUserId, cancellationToken);
        if (!statusResult.IsSuccess)
            throw new Exception("Status integration validation mapped internal framework fallback fault.");

        return IntegrationSuccess(statusResult.Value);
    }

    [Authorize]
    [HttpGet("connect/{service}")]
    public async Task<IActionResult> Connect(string service, [FromQuery] string user_id)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (authenticatedUserId != user_id)
            throw new IntegrationUnauthorizedException("Requested user_id does not match the authenticated access token.");

        if (!Enum.TryParse<AppProvider>(service, true, out var provider))
            throw new IntegrationMissingException($"Unsupported integration service provider: {service}.");

        var urlResponse = await _appConnectionService.GetOAuthConnectUrlAsync(provider, user_id);

        return IntegrationSuccess(new { RedirectUrl = urlResponse.Value });
    }


    [Authorize]
    [HttpGet("github/issues")]
    public async Task<IActionResult> GetGitHubIssues([FromQuery] string state = "open", CancellationToken cancellationToken = default)
    {
        SetActiveProvider(AppProvider.GitHub);
        var result = await _gitHubIntegrationService.GetIssuesAsync(state, cancellationToken);
        return IntegrationSuccess(result);
    }

    [Authorize]
    [HttpGet("github/prs")]
    public async Task<IActionResult> GetGitHubPullRequests(CancellationToken cancellationToken = default)
    {
        SetActiveProvider(AppProvider.GitHub);
        var result = await _gitHubIntegrationService.GetPullRequestsAsync(cancellationToken);
        return IntegrationSuccess(result);
    }

    [Authorize]
    [HttpPost("github/issues")]
    public async Task<IActionResult> CreateGitHubIssue([FromBody] GitHubCreateIssueRequest request, CancellationToken cancellationToken = default)
    {
        SetActiveProvider(AppProvider.GitHub);
        var result = await _gitHubIntegrationService.CreateIssueAsync(request, cancellationToken);
        return IntegrationSuccess(result);
    }

    [Authorize]
    [HttpPost("github/issues/close")]
    public async Task<IActionResult> CloseGitHubIssue([FromBody] GitHubCloseIssueRequest request, CancellationToken cancellationToken = default)
    {
        SetActiveProvider(AppProvider.GitHub);
        var result = await _gitHubIntegrationService.CloseIssueAsync(request, cancellationToken);
        return IntegrationSuccess(result);
    }

    [Authorize]
    [HttpGet("github/repos")]
    public async Task<IActionResult> GetGitHubRepositories(CancellationToken cancellationToken = default)
    {
        SetActiveProvider(AppProvider.GitHub);
        var result = await _gitHubIntegrationService.GetRepositoriesAsync(cancellationToken);
        return IntegrationSuccess(result);
    }

    [Authorize]
    [HttpGet("gmail/inbox")]
    public async Task<IActionResult> GetGmailInbox([FromQuery] int maxResults = 10, CancellationToken cancellationToken = default)
    {
        SetActiveProvider(AppProvider.Google);
        var result = await _gmailIntegrationService.GetInboxAsync(maxResults, cancellationToken);
        return IntegrationSuccess(result);
    }

    [Authorize]
    [HttpGet("gmail/message")]
    public async Task<IActionResult> GetGmailMessage([FromQuery] string messageId, CancellationToken cancellationToken = default)
    {
        SetActiveProvider(AppProvider.Google);
        var result = await _gmailIntegrationService.GetMessageAsync(messageId, cancellationToken);
        return IntegrationSuccess(result);
    }

    [Authorize]
    [HttpPost("gmail/send")]
    public async Task<IActionResult> SendGmailMessage([FromBody] GmailSendRequest request, CancellationToken cancellationToken = default)
    {
        SetActiveProvider(AppProvider.Google);
        var result = await _gmailIntegrationService.SendMessageAsync(request, cancellationToken);
        return IntegrationSuccess(result);
    }

    [Authorize]
    [HttpPost("gmail/reply")]
    public async Task<IActionResult> ReplyGmailMessage([FromBody] GmailReplyRequest request, CancellationToken cancellationToken = default)
    {
        SetActiveProvider(AppProvider.Google);
        var result = await _gmailIntegrationService.ReplyToMessageAsync(request, cancellationToken);
        return IntegrationSuccess(result);
    }

    [Authorize]
    [HttpPost("gmail/draft")]
    public async Task<IActionResult> DraftGmailMessage([FromBody] GmailDraftRequest request, CancellationToken cancellationToken = default)
    {
        SetActiveProvider(AppProvider.Google);
        var result = await _gmailIntegrationService.CreateDraftAsync(request, cancellationToken);
        return IntegrationSuccess(result);
    }

    [Authorize]
    [HttpGet("calendar/events")]
    public async Task<IActionResult> GetCalendarEvents([FromQuery] DateTime? timeMin, [FromQuery] int maxResults = 10, CancellationToken cancellationToken = default)
    {
        SetActiveProvider(AppProvider.Google);
        var result = await _googleCalendarIntegrationService.GetEventsAsync(timeMin, maxResults, cancellationToken);
        return IntegrationSuccess(result);
    }

    [Authorize]
    [HttpPost("calendar/events")]
    public async Task<IActionResult> CreateCalendarEvent([FromBody] CalendarEventCreateRequest request, CancellationToken cancellationToken = default)
    {
        SetActiveProvider(AppProvider.Google);
        var result = await _googleCalendarIntegrationService.CreateEventAsync(request, cancellationToken);
        return IntegrationSuccess(result);
    }

    [Authorize]
    [HttpPut("calendar/events/{eventId}")]
    public async Task<IActionResult> UpdateCalendarEvent(string eventId, [FromBody] CalendarEventUpdateRequest request, CancellationToken cancellationToken = default)
    {
        SetActiveProvider(AppProvider.Google);
        var result = await _googleCalendarIntegrationService.UpdateEventAsync(eventId, request, cancellationToken);
        return IntegrationSuccess(result);
    }

    [Authorize]
    [HttpDelete("calendar/events/{eventId}")]
    public async Task<IActionResult> DeleteCalendarEvent(string eventId, CancellationToken cancellationToken = default)
    {
        SetActiveProvider(AppProvider.Google);
        var result = await _googleCalendarIntegrationService.DeleteEventAsync(eventId, cancellationToken);
        return IntegrationSuccess(result);
    }

    private void SetActiveProvider(AppProvider provider)
    {
        HttpContext.Items["ActiveProvider"] = provider;
    }
}
