using System.Net.Http.Headers;
using System.Text.Json;
using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Contracts.AppConnections;

namespace AI_genda_API.Services.AppConnectionService.Connectors;

public class GitHubConnector : IAppConnector
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubConnector> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;

    private const string GitHubOAuthAuthorizeUrl = "https://github.com/login/oauth/authorize";
    private const string GitHubTokenUrl = "https://github.com/login/oauth/access_token";
    private const string GitHubApiBaseUrl = "https://api.github.com";

    public AppProvider Provider => AppProvider.GitHub;

    public GitHubConnector(HttpClient httpClient, IConfiguration configuration, ILogger<GitHubConnector> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        _clientId = configuration["OAuth:GitHub:ClientId"] ?? throw new ArgumentNullException("GitHub ClientId is missing");
        _clientSecret = configuration["OAuth:GitHub:ClientSecret"] ?? throw new ArgumentNullException("GitHub ClientSecret is missing");
        _redirectUri = configuration["OAuth:GitHub:RedirectUri"] ?? throw new ArgumentNullException("GitHub CallbackPath is missing");

        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AiGenda", "1.0"));
    }

    public string GetAuthorizationUrl(string state, string? metadata = null)
    {
        var scope = "repo read:user user:email";
        
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "redirect_uri", _redirectUri },
            { "scope", scope },
            { "state", state }
        };

        var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{GitHubOAuthAuthorizeUrl}?{query}";
    }

    public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri, CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, GitHubTokenUrl)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                    { "code", code },
                    { "redirect_uri", redirectUri }
                })
            };

            // GitHub returns JSON if we request it explicitly
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);

            if (tokenResponse.TryGetProperty("error", out var errorElement))
            {
                throw new InvalidOperationException($"GitHub OAuth Error: {errorElement.GetString()} - {tokenResponse.GetProperty("error_description").GetString()}");
            }

            var accessToken = tokenResponse.GetProperty("access_token").GetString() ?? throw new InvalidOperationException("No access token in response");
            
            // GitHub access tokens sometimes don't expire for OAuth apps depending on settings. If they do, they'll have expires_in
            var expiresAt = DateTime.UtcNow.AddDays(365); // Default to long living if not specified
            if (tokenResponse.TryGetProperty("expires_in", out var expiresInElement) && expiresInElement.TryGetInt32(out var expiresIn))
            {
                expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
            }

            var refreshToken = tokenResponse.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            var scopes = tokenResponse.TryGetProperty("scope", out var scopeElement) ? 
                scopeElement.GetString()?.Split(',') ?? Array.Empty<string>() : 
                new[] { "repo", "read:user", "user:email" };

            return new OAuthTokenResponse(accessToken, refreshToken, expiresAt, scopes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to exchange GitHub OAuth code for token");
            throw;
        }
    }

    public async Task<OAuthTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, GitHubTokenUrl)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                    { "grant_type", "refresh_token" },
                    { "refresh_token", refreshToken }
                })
            };

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);

            var accessToken = tokenResponse.GetProperty("access_token").GetString() ?? throw new InvalidOperationException("No access token in response");
            
            var expiresAt = DateTime.UtcNow.AddDays(365);
            if (tokenResponse.TryGetProperty("expires_in", out var expiresInElement) && expiresInElement.TryGetInt32(out var expiresIn))
            {
                expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
            }

            var newRefreshToken = tokenResponse.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : refreshToken;

            return new OAuthTokenResponse(accessToken, newRefreshToken, expiresAt, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh GitHub token");
            throw;
        }
    }

    public async Task<string> GetExternalAccountIdAsync(string accessToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{GitHubApiBaseUrl}/user");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var userElement = JsonSerializer.Deserialize<JsonElement>(content);
        
        return userElement.GetProperty("id").GetInt64().ToString();
    }

    public async Task<SyncResult> SyncAsync(string accessToken, DateTime? lastSyncTime, string? metadata, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting MVP GitHub Sync processing...");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var syncedItems = new List<LinkedDataItem>();

            // 1. جلب المستودعات (Repositories) الخاصة بالمستخدم
            var reposResponse = await _httpClient.GetAsync($"{GitHubApiBaseUrl}/user/repos?sort=updated&per_page=30", cancellationToken);
            if (reposResponse.IsSuccessStatusCode)
            {
                var reposContent = await reposResponse.Content.ReadAsStringAsync(cancellationToken);
                using var jsonDoc = JsonDocument.Parse(reposContent);
                foreach (var repo in jsonDoc.RootElement.EnumerateArray())
                {
                    var externalId = repo.GetProperty("id").GetInt64().ToString();
                    var fullName = repo.GetProperty("full_name").GetString() ?? "Unknown Repo";
                    var rawData = JsonSerializer.Serialize(repo);

                    syncedItems.Add(new LinkedDataItem(
                        ExternalId: externalId,
                        DataType: "GitHubRepository",
                        Summary: fullName,
                        StartTime: null,
                        EndTime: null,
                        RawData: rawData
                    ));
                }
            }

            // 2. جلب الـ Issues المفتوحة المرتبطة بالمستخدم
            var issuesResponse = await _httpClient.GetAsync($"{GitHubApiBaseUrl}/user/issues?filter=all&state=open&per_page=30", cancellationToken);
            if (issuesResponse.IsSuccessStatusCode)
            {
                var issuesContent = await issuesResponse.Content.ReadAsStringAsync(cancellationToken);
                using var jsonDoc = JsonDocument.Parse(issuesContent);
                foreach (var issue in jsonDoc.RootElement.EnumerateArray())
                {
                    var externalId = issue.GetProperty("id").GetInt64().ToString();
                    var title = issue.GetProperty("title").GetString() ?? "Untitled Issue";
                    var rawData = JsonSerializer.Serialize(issue);

                    syncedItems.Add(new LinkedDataItem(
                        ExternalId: externalId,
                        DataType: "GitHubIssue",
                        Summary: title,
                        StartTime: null,
                        EndTime: null,
                        RawData: rawData
                    ));
                }
            }

            return new SyncResult(
                Success: true,
                RecordsSynced: syncedItems.Count,
                Data: syncedItems,
                Error: null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub Sync execution failed.");
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
            var request = new HttpRequestMessage(HttpMethod.Get, $"{GitHubApiBaseUrl}/user");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
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
            // GitHub requires deleting the grant using Client ID and Secret in Basic Auth
            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.github.com/applications/{_clientId}/grant");
            
            var authValue = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            
            request.Content = new StringContent(JsonSerializer.Serialize(new { access_token = accessToken }));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}