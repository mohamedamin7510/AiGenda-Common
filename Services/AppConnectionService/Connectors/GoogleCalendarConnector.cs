using System.Text.Json;
using AI_genda_API.Abstractions.Enums;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

namespace AI_genda_API.Services.AppConnectionService.Connectors;

/// <summary>
/// Google Calendar connector for OAuth and event syncing
/// </summary>
public class GoogleCalendarConnector : IAppConnector
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleCalendarConnector> _logger;

    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;
    private const string GoogleOAuthAuthorizeUrl = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string GoogleTokenUrl = "https://oauth2.googleapis.com/token";
    private const string GoogleCalendarApiUrl = "https://www.googleapis.com/calendar/v3";

    public AppProvider Provider => AppProvider.Google;

    public GoogleCalendarConnector(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GoogleCalendarConnector> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _clientId = configuration["OAuth:Google:ClientId"] ?? throw new InvalidOperationException("Google OAuth ClientId not configured");
        _clientSecret = configuration["OAuth:Google:ClientSecret"] ?? throw new InvalidOperationException("Google OAuth ClientSecret not configured");
        _redirectUri = configuration["OAuth:Google:RedirectUri"] ?? throw new InvalidOperationException("Google OAuth RedirectUri not configured");
    }

    public string GetAuthorizationUrl(string state, string? metadata = null)
    {
        var scope = string.Join(" ", new[]
        {
            "https://www.googleapis.com/auth/calendar",
            "https://www.googleapis.com/auth/userinfo.email",
            "https://www.googleapis.com/auth/gmail.readonly",
            "https://www.googleapis.com/auth/gmail.send",
            "https://www.googleapis.com/auth/gmail.modify"
        });
        
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "redirect_uri", _redirectUri },
            { "response_type", "code" },
            { "scope", scope },
            { "state", state },
            { "access_type", "offline" },
            { "prompt", "consent" }
        };

        var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{GoogleOAuthAuthorizeUrl}?{query}";
    }

    public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri, CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, GoogleTokenUrl)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                })
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);

            var accessToken = tokenResponse.GetProperty("access_token").GetString() ?? throw new InvalidOperationException("No access token in response");
            var expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();
            var refreshToken = tokenResponse.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;

            return new OAuthTokenResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                ExpiresAt: DateTime.UtcNow.AddSeconds(expiresIn),
                Scopes: new[] { "calendar", "userinfo", "gmail.readonly", "gmail.send", "gmail.modify" }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to exchange Google OAuth code for token");
            throw;
        }
    }

    public async Task<OAuthTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, GoogleTokenUrl)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                    { "refresh_token", refreshToken },
                    { "grant_type", "refresh_token" }
                })
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);

            var accessToken = tokenResponse.GetProperty("access_token").GetString() ?? throw new InvalidOperationException("No access token in response");
            var expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();

            return new OAuthTokenResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                ExpiresAt: DateTime.UtcNow.AddSeconds(expiresIn),
                Scopes: new[] { "calendar", "userinfo", "gmail.readonly", "gmail.send", "gmail.modify" }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh Google OAuth token");
            throw;
        }
    }

    public async Task<string> GetExternalAccountIdAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var userInfo = JsonSerializer.Deserialize<JsonElement>(content);

            return userInfo.GetProperty("email").GetString() ?? throw new InvalidOperationException("No email in response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Google account ID");
            throw;
        }
    }

    public async Task<SyncResult> SyncAsync(string accessToken, DateTime? lastSyncTime, string? metadata, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting Google Calendar sync with lastSyncTime: {LastSyncTime}", lastSyncTime);

            var credential = GoogleCredential.FromAccessToken(accessToken);
            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "AIgenda"
            });

            var events = new List<LinkedDataItem>();
            string? pageToken = null;

            do
            {
                var request = service.Events.List("primary");

                request.TimeMinDateTimeOffset = DateTime.UtcNow.AddDays(-30);
                request.TimeMaxDateTimeOffset = DateTime.UtcNow.AddDays(30);
                request.ShowDeleted = false;
                request.SingleEvents = true; // Expands recurring events into instances
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
                request.MaxResults = 250;
                request.PageToken = pageToken;

                var response = await request.ExecuteAsync(cancellationToken);

                if (response.Items != null)
                {
                    foreach (var calendarEvent in response.Items)
                    {
                        // Skip events without summary
                        var summary = calendarEvent.Summary ?? "Untitled Event";

                        // Handle start and end times
                        // For all-day events, Start.Date and End.Date are used instead of DateTime
                        DateTime? startTime = null;
                        DateTime? endTime = null;

                        if (calendarEvent.Start?.DateTimeDateTimeOffset != null)
                            startTime = calendarEvent.Start.DateTimeDateTimeOffset.Value.UtcDateTime;
                        else if (calendarEvent.Start?.Date != null && DateTime.TryParse(calendarEvent.Start.Date, out var startParsed))
                            startTime = startParsed.ToUniversalTime();

                        if (calendarEvent.End?.DateTimeDateTimeOffset != null)
                            endTime = calendarEvent.End.DateTimeDateTimeOffset.Value.UtcDateTime;
                        else if (calendarEvent.End?.Date != null && DateTime.TryParse(calendarEvent.End.Date, out var endParsed))
                            endTime = endParsed.ToUniversalTime();

                        var rawData = JsonSerializer.Serialize(calendarEvent);

                        events.Add(new LinkedDataItem(
                            ExternalId: calendarEvent.Id,
                            DataType: "CalendarEvent",
                            Summary: summary,
                            StartTime: startTime,
                            EndTime: endTime,
                            RawData: rawData
                        ));
                    }
                }

                pageToken = response.NextPageToken;

            } while (pageToken != null && !cancellationToken.IsCancellationRequested);

            _logger.LogInformation("Completed Google Calendar sync. Found {Count} events.", events.Count);

            return new SyncResult(
                Success: true,
                RecordsSynced: events.Count,
                Data: events
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync Google Calendar");
            return new SyncResult(
                Success: false,
                RecordsSynced: 0,
                Data: new List<LinkedDataItem>(),
                Error: ex.Message
            );
        }
    }

    public async Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RevokeAccessAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://oauth2.googleapis.com/revoke?token={accessToken}");
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke Google OAuth token");
            return false;
        }
    }
}
