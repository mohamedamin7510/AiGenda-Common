using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Contracts.AppConnections;

namespace AI_genda_API.Services.AppConnectionService;

public interface IAppConnectionService
{
    /// <summary>
    /// Gets the current integration status mapping whether connections are active.
    /// </summary>
    Task<Result<IntegrationsStatusResponse>> GetIntegrationsStatusAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new app connection after OAuth callback
    /// </summary>
    Task<Result<AppConnectionResponse>> CreateConnectionAsync(
        int? workspaceId,
        string userId,
        AppProvider provider,
        string code,
        string redirectUri,
        string? metadata = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all connections for the user
    /// </summary>
    Task<Result<IEnumerable<AppConnectionResponse>>> GetConnectionsAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific connection
    /// </summary>
    Task<Result<AppConnectionDetailResponse>> GetConnectionAsync(
        string connectionId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates connection settings
    /// </summary>
    Task<Result<AppConnectionResponse>> UpdateConnectionAsync(
        string connectionId,
        string userId,
        AppConnectionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects and removes a connection
    /// </summary>
    Task<Result> DisconnectAsync(
        string connectionId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually triggers a sync for a connection
    /// </summary>
    Task<Result<SyncStatusResponse>> SyncNowAsync(
        string connectionId,
        string userId,
        bool forceFullSync = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the sync status of a connection
    /// </summary>
    Task<Result<SyncStatusResponse>> GetSyncStatusAsync(
        string connectionId,
        string userId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the OAuth Connect authorization URL for a provider tracking secure state
    /// </summary>
    Task<Result<string>> GetOAuthConnectUrlAsync(AppProvider provider, string userId);

    /// <summary>
    /// Exchanges an OAuth callback code mapping securely back to the originating state parameter
    /// </summary>
    Task<Result<AppConnectionResponse>> HandleOAuthCallbackAsync(string code, string state, string redirectUri);
    
    /// <summary>
    /// Syncs all active connections (called by Hangfire jobs)
    /// </summary>
    Task<int> SyncAllActiveConnectionsAsync(AppProvider? filterByProvider = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Syncs connections for a specific user
    /// </summary>
    Task<int> SyncUserConnectionsAsync(string userId, CancellationToken cancellationToken = default);
}
