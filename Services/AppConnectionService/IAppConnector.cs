using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Contracts.AppConnections;

namespace AI_genda_API.Services.AppConnectionService;

/// <summary>
/// Interface for platform-specific connectors (Google, GitHub, etc.)
/// </summary>
public interface IAppConnector
{
    /// <summary>
    /// Gets the provider this connector handles
    /// </summary>
    AppProvider Provider { get; }
    
    /// <summary>
    /// Generates the OAuth authorization URL
    /// </summary>
    string GetAuthorizationUrl(string state, string? metadata = null);
    
    /// <summary>
    /// Exchanges authorization code for tokens
    /// </summary>
    Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri, CancellationToken cancellationToken);
    
    /// <summary>
    /// Refreshes expired access token
    /// </summary>
    Task<OAuthTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    
    /// <summary>
    /// Gets the external account identifier (email, username, etc.)
    /// </summary>
    Task<string> GetExternalAccountIdAsync(string accessToken, CancellationToken cancellationToken);
    
    /// <summary>
    /// Syncs data from external provider
    /// </summary>
    Task<SyncResult> SyncAsync(string accessToken, DateTime? lastSyncTime, string? metadata, CancellationToken cancellationToken);
    
    /// <summary>
    /// Verifies token validity
    /// </summary>
    Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken);
    
    /// <summary>
    /// Revokes access (disconnect the app)
    /// </summary>
    Task<bool> RevokeAccessAsync(string accessToken, CancellationToken cancellationToken);
}

public record OAuthTokenResponse(
    string AccessToken,
    string? RefreshToken,
    DateTime ExpiresAt,
    string[] Scopes
);

public record SyncResult(
    bool Success,
    int RecordsSynced,
    List<LinkedDataItem> Data,
    string? Error = null
);

public record LinkedDataItem(
    string ExternalId,
    string DataType,
    string Summary,
    DateTime? StartTime,
    DateTime? EndTime,
    string RawData
);
