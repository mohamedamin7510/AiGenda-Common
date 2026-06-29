using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Abstractions.Errors;
using AI_genda_API.Contracts.AppConnections;
using AI_genda_API.Entities;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace AI_genda_API.Services.AppConnectionService;

public class AppConnectionService : IAppConnectionService
{
    private readonly AppContext _context;
    private readonly IAppConnectorFactory _connectorFactory;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger<AppConnectionService> _logger;

    public AppConnectionService(
        AppContext context,
        IAppConnectorFactory connectorFactory,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<AppConnectionService> logger)
    {
        _context = context;
        _connectorFactory = connectorFactory;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public async Task<Result<IntegrationsStatusResponse>> GetIntegrationsStatusAsync(string userId, CancellationToken cancellationToken = default)
    {
        var activeConnections = await _context.AppConnections
            .Where(x => x.UserId == userId && x.IsActive)
            .Select(x => x.Provider)
            .ToListAsync(cancellationToken);

        bool githubActive = activeConnections.Contains(AppProvider.GitHub);
        bool googleActive = activeConnections.Contains(AppProvider.Google);

        return Result<IntegrationsStatusResponse>.Success(new IntegrationsStatusResponse(githubActive, googleActive, googleActive));
    }

    private class OAuthStatePayload
    {
        public string UserId { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }

    public Task<Result<string>> GetOAuthConnectUrlAsync(AppProvider provider, string userId)
    {
        var protector = _dataProtectionProvider.CreateProtector("OAuthStateProtector_V1");

        var statePayload = System.Text.Json.JsonSerializer.Serialize(new OAuthStatePayload
        {
            UserId = userId,
            Provider = provider.ToString(),
            GeneratedAt = DateTime.UtcNow
        });

        var encryptedState = protector.Protect(statePayload);

        var connector = _connectorFactory.CreateConnector(provider);
        var url = connector.GetAuthorizationUrl(encryptedState);

        return System.Threading.Tasks.Task.FromResult(Result.Success(url));
    }

    public async Task<Result<AppConnectionResponse>> HandleOAuthCallbackAsync(string code, string state, string redirectUri)
    {
        OAuthStatePayload payload;

        var protector = _dataProtectionProvider.CreateProtector("OAuthStateProtector_V1");

        string decryptedState;
        try
        {
            decryptedState = protector.Unprotect(state);
        }
        catch
        {
            throw new AI_genda_API.Exceptions.IntegrationUnauthorizedException("Invalid OAuth state parameter.");
        }

        payload = JsonSerializer.Deserialize<OAuthStatePayload>(decryptedState)!;
        if (payload == null || payload.GeneratedAt < DateTime.UtcNow.AddMinutes(-30))
            throw new AI_genda_API.Exceptions.IntegrationUnauthorizedException("OAuth state expired or malformed.");

        if (!Enum.TryParse<AppProvider>(payload.Provider, out var provider))
            throw new AI_genda_API.Exceptions.IntegrationMissingException($"Unsupported integration provider: {payload.Provider}");

        return await CreateConnectionAsync(
             workspaceId: null,
             userId: payload.UserId,
             provider: provider,
             code: code,
             redirectUri: redirectUri,
             metadata: null,
             cancellationToken: default
        );
    }

    /// <summary>
    /// Creates a new app connection or updates an existing one transparently (Upsert Pattern).
    /// </summary>
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

            // 1. تبادل الكود وجلب التوكنز والمعرف الخارجي أولاً
            var connector = _connectorFactory.CreateConnector(provider);
            var tokenResponse = await connector.ExchangeCodeForTokenAsync(code, redirectUri, cancellationToken);
            var externalAccountId = await connector.GetExternalAccountIdAsync(tokenResponse.AccessToken, cancellationToken);

            // 2. التحقق من وجود اتصال نشط قديم لنفس المستخدم ونفس المنصة
            var existingConnection = await _context.AppConnections
                .Where(c => c.UserId == userId && c.Provider == provider && c.IsActive)
                .SingleOrDefaultAsync(cancellationToken);

            if (existingConnection is not null)
            {
                // حفظ كـ Plain text صافي ومباشر لتجنب مشاكل التشفير على السيرفر المشترك
                existingConnection.AccessToken = tokenResponse.AccessToken;
                existingConnection.RefreshToken = string.IsNullOrEmpty(tokenResponse.RefreshToken)
                    ? existingConnection.RefreshToken
                    : tokenResponse.RefreshToken;
                existingConnection.TokenExpiresAt = tokenResponse.ExpiresAt;
                existingConnection.ExternalAccountId = externalAccountId;
                existingConnection.GrantedScopes = string.Join(" ", tokenResponse.Scopes);

                if (!string.IsNullOrEmpty(metadata))
                    existingConnection.Metadata = metadata;

                existingConnection.UpdatedAt = DateTime.UtcNow;
                existingConnection.UpdatedById = userId;

                _context.AppConnections.Update(existingConnection);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Updated existing app connection tokens for user {UserId} provider {Provider}",
                    userId, provider);

                return Result.Success(MapToResponse(existingConnection));
            }

            // 3. إذا لم يكن الحساب موجوداً، ننشئ اتصالاً جديداً بالكامل كـ Plain text
            var connection = new AppConnection
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                WorkSpaceId = workspaceId > 0 ? workspaceId : null,
                Provider = provider,
                ExternalAccountId = externalAccountId,
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = string.IsNullOrEmpty(tokenResponse.RefreshToken)
                    ? string.Empty
                    : tokenResponse.RefreshToken,
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
                var decryptedToken = connection.AccessToken;
                var connector = _connectorFactory.CreateConnector(connection.Provider);
                await connector.RevokeAccessAsync(decryptedToken, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to revoke access for connection {ConnectionId}", connectionId);
            }

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
                .Take(10)
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

    private async Task<int> SyncConnectionAsync(
        AppConnection connection,
        bool forceFullSync,
        CancellationToken cancellationToken)
    {
        connection.SyncStatus = SyncStatus.Syncing;
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var accessToken = connection.AccessToken;

            if (connection.TokenExpiresAt.HasValue && connection.TokenExpiresAt <= DateTime.UtcNow.AddMinutes(5))
            {
                if (connection.RefreshToken is not null)
                {
                    accessToken = await RefreshAccessTokenAsync(connection, cancellationToken);
                }
            }

            var connector = _connectorFactory.CreateConnector(connection.Provider);
            var lastSyncTime = forceFullSync ? null : connection.LastSyncAt;
            var syncResult = await connector.SyncAsync(accessToken, lastSyncTime, connection.Metadata, cancellationToken);

            if (syncResult.Success)
            {
                // 1. إضافة الداتا الجديدة بالكامل في ذاكرة الـ Context أولاً
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

                // 2. الداتا الجديدة تم تجهيزها بسلام في الميموري؛ الآن نسحب البيانات القديمة الخاصة بهذا الاتصال لتجهيزها للحذف
                var oldData = await _context.LinkedData
                    .Where(ld => ld.AppConnectionId == connection.Id)
                    .ToListAsync(cancellationToken);

                _context.LinkedData.RemoveRange(oldData);

                // 3. تحديث بيانات حالة الاتصال بنجاح
                connection.LastSyncAt = DateTime.UtcNow;
                connection.SyncStatus = SyncStatus.Success;
                connection.LastSyncError = null;

                _logger.LogInformation(
                    "Successfully prepared {RecordCount} records for safe swap from {Provider} for user {UserId}",
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

            // 4. هنا يتم تنفيذ الـ Transaction كاملة (حذف القديم وإضافة الجديد معاً) بسلام عبر الـ Database
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
        CancellationToken cancellationToken)
    {
        try
        {
            var decryptedRefreshToken = connection.RefreshToken!;
            var connector = _connectorFactory.CreateConnector(connection.Provider);
            var newTokenResponse = await connector.RefreshTokenAsync(decryptedRefreshToken, cancellationToken);

            connection.AccessToken = newTokenResponse.AccessToken;
            connection.RefreshToken = newTokenResponse.RefreshToken ?? decryptedRefreshToken;
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
            connection.Metadata,
            connection.TokenExpiresAt
        );
    }
}