using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Abstractions.Errors;
using AI_genda_API.Contracts.AppConnections;
using AI_genda_API.Entities;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace AI_genda_API.Services.AppConnectionService;

public class AppConnectionService : IAppConnectionService
{
    private readonly AppContext _context;
    private readonly IAppConnectorFactory _connectorFactory;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AppConnectionService> _logger;
    private readonly IConfiguration _configuration;

    public AppConnectionService(
        AppContext context,
        IAppConnectorFactory connectorFactory,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AppConnectionService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _connectorFactory = connectorFactory;
        _dataProtectionProvider = dataProtectionProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _configuration = configuration;
    }

    public string GetAuthorizationUrl(AppProvider provider, string state)
    {
        try
        {
            // Validate configuration
            var baseUrl = _configuration["BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
                throw new InvalidOperationException("BaseUrl is not configured in appsettings.json");

            var connector = _connectorFactory.CreateConnector(provider);
            return connector.GetAuthorizationUrl(state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate authorization URL for provider: {Provider}", provider);
            throw;
        }
    }

    public async Task<Result<AppConnectionResponse>> CreateConnectionAsync(
        int? workspaceId,
        string userId,
        AppProvider provider,
        string code,
        string redirectUri,
        string? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (workspaceId.HasValue && workspaceId > 0)
            {
                // Verify workspace access (because AuthCallbackController cannot use [HasPermission] route filters)
                var workspace = await _context.WorkSpaces
                    .Where(w => w.Id == workspaceId && w.RemovedAt == null)
                    .Select(w => new { w.Id, w.CreatedById })
                    .SingleOrDefaultAsync(cancellationToken);

                if (workspace is null)
                    return Result.Faluire<AppConnectionResponse>(WorkSpaceErrors.WorkSpaceNotFound);

                bool hasAccess = workspace.CreatedById == userId;

                if (!hasAccess)
                {
                    var memberPermissions = await _context.WorkspaceMembers
                        .Where(m => m.WrokSpaceID == workspaceId && m.UserID == userId)
                        .Select(m => m.Permissions)
                        .SingleOrDefaultAsync(cancellationToken);

                    if (memberPermissions != null && memberPermissions.Any(p => string.Equals(p, AI_genda_API.Abstractions.Const.Permissions.UpdateWorkSpaces, StringComparison.OrdinalIgnoreCase)))
                    {
                        hasAccess = true;
                    }
                }

                if (!hasAccess)
                    return Result.Faluire<AppConnectionResponse>(WorkspaceMemberErrors.AccessDenied);
            }

            // Check if connection already exists for this user/provider
            var existingConnection = await _context.AppConnections
                .Where(c => c.UserId == userId && c.Provider == provider && c.IsActive)
                .SingleOrDefaultAsync(cancellationToken);

            if (existingConnection is not null)
                return Result.Faluire<AppConnectionResponse>(AppConnectionErrors.ConnectionAlreadyExists);

            // Exchange code for tokens
            var connector = _connectorFactory.CreateConnector(provider);
            var tokenResponse = await connector.ExchangeCodeForTokenAsync(code, redirectUri, cancellationToken);

            // Get external account ID
            var externalAccountId = await connector.GetExternalAccountIdAsync(tokenResponse.AccessToken, cancellationToken);

            // Encrypt tokens
            var protector = _dataProtectionProvider.CreateProtector("AppConnection.Tokens");
            var encryptedAccessToken = protector.Protect(tokenResponse.AccessToken);
            var encryptedRefreshToken = tokenResponse.RefreshToken is not null 
                ? protector.Protect(tokenResponse.RefreshToken) 
                : null;

            // Create connection
            var connection = new AppConnection
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                WorkSpaceId = workspaceId > 0 ? workspaceId : null,
                Provider = provider,
                ExternalAccountId = externalAccountId,
                AccessToken = encryptedAccessToken,
                RefreshToken = encryptedRefreshToken,
                TokenExpiresAt = tokenResponse.ExpiresAt,
                IsActive = true,
                SyncFrequency = SyncFrequency.Daily,
                SyncStatus = SyncStatus.Pending,
                GrantedScopes = string.Join(" ", tokenResponse.Scopes),
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId,
                UpdatedById = userId,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.AppConnections.AddAsync(connection, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created app connection for user {UserId} provider {Provider} workspace {WorkspaceId}",
                userId, provider, workspaceId);

            return Result.Success(MapToResponse(connection));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create app connection for provider: {Provider}", provider);
            return Result.Faluire<AppConnectionResponse>(new Error(
                "CREATE_CONNECTION_FAILED",
                $"Failed to create connection: {ex.Message}",
                500));
        }
    }

    public async Task<Result<IEnumerable<AppConnectionResponse>>> GetConnectionsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connections = await _context.AppConnections
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);

            var responses = connections.Select(MapToResponse).ToList();
            return Result.Success<IEnumerable<AppConnectionResponse>>(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get app connections for user {UserId}", userId);
            return Result.Faluire<IEnumerable<AppConnectionResponse>>(new Error(
                "GET_CONNECTIONS_FAILED",
                "Failed to retrieve connections",
                500));
        }
    }

    public async Task<Result<AppConnectionDetailResponse>> GetConnectionAsync(
        string connectionId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _context.AppConnections
                .Where(c => c.Id == connectionId && c.UserId == userId)
                .SingleOrDefaultAsync(cancellationToken);

            if (connection is null)
                return Result.Faluire<AppConnectionDetailResponse>(AppConnectionErrors.ConnectionNotFound);

            return Result.Success(MapToDetailResponse(connection));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get app connection {ConnectionId}", connectionId);
            return Result.Faluire<AppConnectionDetailResponse>(new Error(
                "GET_CONNECTION_FAILED",
                "Failed to retrieve connection",
                500));
        }
    }

    public async Task<Result<AppConnectionResponse>> UpdateConnectionAsync(
        string connectionId,
        string userId,
        AppConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _context.AppConnections
                .Where(c => c.Id == connectionId && c.UserId == userId)
                .SingleOrDefaultAsync(cancellationToken);

            if (connection is null)
                return Result.Faluire<AppConnectionResponse>(AppConnectionErrors.ConnectionNotFound);

            connection.SyncFrequency = request.SyncFrequency;
            if (!string.IsNullOrEmpty(request.Metadata))
                connection.Metadata = request.Metadata;
            connection.UpdatedAt = DateTime.UtcNow;
            connection.UpdatedById = userId;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated app connection {ConnectionId}", connectionId);
            return Result.Success(MapToResponse(connection));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update app connection {ConnectionId}", connectionId);
            return Result.Faluire<AppConnectionResponse>(new Error(
                "UPDATE_CONNECTION_FAILED",
                "Failed to update connection",
                500));
        }
    }

    public async Task<Result> DisconnectAsync(
        string connectionId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _context.AppConnections
                .Where(c => c.Id == connectionId && c.UserId == userId)
                .SingleOrDefaultAsync(cancellationToken);

            if (connection is null)
                return Result.Faluire(AppConnectionErrors.ConnectionNotFound);

            try
            {
                // Attempt to revoke access from external provider
                var protector = _dataProtectionProvider.CreateProtector("AppConnection.Tokens");
                var decryptedToken = protector.Unprotect(connection.AccessToken);
                var connector = _connectorFactory.CreateConnector(connection.Provider);
                await connector.RevokeAccessAsync(decryptedToken, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to revoke access for connection {ConnectionId}", connectionId);
                // Continue with disconnection even if revocation fails
            }

            // Delete connection and linked data
            _context.AppConnections.Remove(connection);
            var linkedData = await _context.LinkedData
                .Where(ld => ld.AppConnectionId == connectionId)
                .ToListAsync(cancellationToken);
            _context.LinkedData.RemoveRange(linkedData);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Disconnected app connection {ConnectionId} provider {Provider}", 
                connectionId, connection.Provider);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect app connection {ConnectionId}", connectionId);
            return Result.Faluire(new Error(
                "DISCONNECT_FAILED",
                "Failed to disconnect",
                500));
        }
    }

    public async Task<Result<SyncStatusResponse>> SyncNowAsync(
        string connectionId,
        string userId,
        bool forceFullSync = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _context.AppConnections
                .Where(c => c.Id == connectionId && c.UserId == userId)
                .SingleOrDefaultAsync(cancellationToken);

            if (connection is null)
                return Result.Faluire<SyncStatusResponse>(AppConnectionErrors.ConnectionNotFound);

            if (!connection.IsActive)
                return Result.Faluire<SyncStatusResponse>(new Error(
                    "CONNECTION_INACTIVE",
                    "Connection is not active",
                    400));

            connection.SyncStatus = SyncStatus.Syncing;
            await _context.SaveChangesAsync(cancellationToken);

            try
            {
                var syncResult = await SyncConnectionAsync(connection, forceFullSync, cancellationToken);
                return Result.Success(new SyncStatusResponse(
                    connection.Id,
                    connection.Provider,
                    connection.SyncStatus,
                    connection.LastSyncAt,
                    connection.LastSyncError,
                    syncResult,
                    DateTime.UtcNow
                ));
            }
            catch (Exception ex)
            {
                connection.SyncStatus = SyncStatus.Failed;
                connection.LastSyncError = ex.Message;
                await _context.SaveChangesAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync app connection {ConnectionId}", connectionId);
            return Result.Faluire<SyncStatusResponse>(new Error(
                "SYNC_FAILED",
                $"Sync failed: {ex.Message}",
                500));
        }
    }

    public async Task<Result<SyncStatusResponse>> GetSyncStatusAsync(
        string connectionId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _context.AppConnections
                .Where(c => c.Id == connectionId && c.UserId == userId)
                .SingleOrDefaultAsync(cancellationToken);

            if (connection is null)
                return Result.Faluire<SyncStatusResponse>(AppConnectionErrors.ConnectionNotFound);

            var recordCount = await _context.LinkedData
                .CountAsync(ld => ld.AppConnectionId == connectionId, cancellationToken);

            return Result.Success(new SyncStatusResponse(
                connection.Id,
                connection.Provider,
                connection.SyncStatus,
                connection.LastSyncAt,
                connection.LastSyncError,
                recordCount,
                DateTime.UtcNow
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sync status for connection {ConnectionId}", connectionId);
            return Result.Faluire<SyncStatusResponse>(new Error(
                "GET_SYNC_STATUS_FAILED",
                "Failed to retrieve sync status",
                500));
        }
    }

    public async Task<int> SyncAllActiveConnectionsAsync(
        AppProvider? filterByProvider = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting sync for all active connections. Filter: {Filter}", filterByProvider);

            var query = _context.AppConnections
                .Where(c => c.IsActive && c.SyncStatus != SyncStatus.Syncing);

            if (filterByProvider.HasValue)
                query = query.Where(c => c.Provider == filterByProvider);

            var connections = await query
                .OrderBy(c => c.LastSyncAt ?? DateTime.MinValue)
                .Take(10) // Limit concurrent syncs
                .ToListAsync(cancellationToken);

            int totalSynced = 0;
            foreach (var connection in connections)
            {
                try
                {
                    await SyncConnectionAsync(connection, false, cancellationToken);
                    totalSynced++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to sync connection {ConnectionId}", connection.Id);
                }
            }

            _logger.LogInformation("Completed sync for {TotalSynced} connections", totalSynced);
            return totalSynced;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync all active connections");
            return 0;
        }
    }

    public async Task<int> SyncUserConnectionsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connections = await _context.AppConnections
                .Where(c => c.UserId == userId && c.IsActive)
                .ToListAsync(cancellationToken);

            int totalSynced = 0;
            foreach (var connection in connections)
            {
                try
                {
                    await SyncConnectionAsync(connection, false, cancellationToken);
                    totalSynced++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to sync user connection {ConnectionId}", connection.Id);
                }
            }

            return totalSynced;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync user connections for {UserId}", userId);
            return 0;
        }
    }

    // ===== Private Helpers =====

    private async Task<int> SyncConnectionAsync(
        AppConnection connection,
        bool forceFullSync,
        CancellationToken cancellationToken)
    {
        connection.SyncStatus = SyncStatus.Syncing;
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var protector = _dataProtectionProvider.CreateProtector("AppConnection.Tokens");
            var accessToken = protector.Unprotect(connection.AccessToken);

            // Check if token needs refresh
            if (connection.TokenExpiresAt.HasValue && connection.TokenExpiresAt <= DateTime.UtcNow.AddMinutes(5))
            {
                if (connection.RefreshToken is not null)
                {
                    accessToken = await RefreshAccessTokenAsync(connection, protector, cancellationToken);
                }
            }

            var connector = _connectorFactory.CreateConnector(connection.Provider);
            var lastSyncTime = forceFullSync ? null : connection.LastSyncAt;
            var syncResult = await connector.SyncAsync(accessToken, lastSyncTime, connection.Metadata, cancellationToken);

            if (syncResult.Success)
            {
                // Store synced data
                foreach (var item in syncResult.Data)
                {
                    var linkedData = new LinkedData
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = connection.UserId,
                        WorkSpaceId = connection.WorkSpaceId,
                        AppConnectionId = connection.Id,
                        Provider = connection.Provider,
                        DataType = item.DataType,
                        ExternalId = item.ExternalId,
                        Summary = item.Summary,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        RawData = item.RawData,
                        SyncedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = connection.UserId
                    };

                    await _context.LinkedData.AddAsync(linkedData, cancellationToken);
                }

                connection.LastSyncAt = DateTime.UtcNow;
                connection.SyncStatus = SyncStatus.Success;
                connection.LastSyncError = null;

                _logger.LogInformation(
                    "Successfully synced {RecordCount} records from {Provider} for user {UserId}",
                    syncResult.RecordsSynced, connection.Provider, connection.UserId);
            }
            else
            {
                connection.SyncStatus = SyncStatus.Failed;
                connection.LastSyncError = syncResult.Error ?? "Unknown sync error";

                _logger.LogWarning(
                    "Sync failed for connection {ConnectionId} provider {Provider}: {Error}",
                    connection.Id, connection.Provider, connection.LastSyncError);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return syncResult.RecordsSynced;
        }
        catch (Exception ex)
        {
            connection.SyncStatus = SyncStatus.Failed;
            connection.LastSyncError = ex.Message;
            await _context.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    private async Task<string> RefreshAccessTokenAsync(
        AppConnection connection,
        IDataProtector protector,
        CancellationToken cancellationToken)
    {
        try
        {
            var decryptedRefreshToken = protector.Unprotect(connection.RefreshToken!);
            var connector = _connectorFactory.CreateConnector(connection.Provider);
            var newTokenResponse = await connector.RefreshTokenAsync(decryptedRefreshToken, cancellationToken);

            connection.AccessToken = protector.Protect(newTokenResponse.AccessToken);
            connection.RefreshToken = protector.Protect(newTokenResponse.RefreshToken ?? decryptedRefreshToken);
            connection.TokenExpiresAt = newTokenResponse.ExpiresAt;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Refreshed access token for connection {ConnectionId}", connection.Id);
            return newTokenResponse.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh access token for connection {ConnectionId}", connection.Id);
            throw;
        }
    }

    private AppConnectionResponse MapToResponse(AppConnection connection)
    {
        return new AppConnectionResponse(
            connection.Id,
            connection.Provider,
            connection.ExternalAccountId,
            connection.SyncFrequency,
            connection.SyncStatus,
            connection.LastSyncAt,
            connection.LastSyncError,
            connection.IsActive,
            connection.CreatedAt
        );
    }

    private AppConnectionDetailResponse MapToDetailResponse(AppConnection connection)
    {
        return new AppConnectionDetailResponse(
            connection.Id,
            connection.Provider,
            connection.ExternalAccountId,
            connection.SyncFrequency,
            connection.SyncStatus,
            connection.LastSyncAt,
            connection.LastSyncError,
            connection.IsActive,
            connection.CreatedAt,
            connection.GrantedScopes,
            connection.Metadata
        );
    }
}
