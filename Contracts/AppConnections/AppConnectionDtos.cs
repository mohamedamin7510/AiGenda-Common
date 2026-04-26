using AI_genda_API.Abstractions.Enums;

namespace AI_genda_API.Contracts.AppConnections;

public record AppConnectionRequest(
    AppProvider Provider,
    SyncFrequency SyncFrequency = SyncFrequency.Daily,
    string? Metadata = null
);

public record AppConnectionResponse(
    string Id,
    AppProvider Provider,
    string ExternalAccountId,
    SyncFrequency SyncFrequency,
    SyncStatus SyncStatus,
    DateTime? LastSyncAt,
    string? LastSyncError,
    bool IsActive,
    DateTime CreatedAt
);

public record AppConnectionDetailResponse(
    string Id,
    AppProvider Provider,
    string ExternalAccountId,
    SyncFrequency SyncFrequency,
    SyncStatus SyncStatus,
    DateTime? LastSyncAt,
    string? LastSyncError,
    bool IsActive,
    DateTime CreatedAt,
    string? GrantedScopes,
    string? Metadata
);

public record SyncStatusResponse(
    string ConnectionId,
    AppProvider Provider,
    SyncStatus Status,
    DateTime? LastSyncAt,
    string? LastSyncError,
    int? RecordsSynced,
    DateTime CheckedAt
);

public record ManualSyncRequest(
    bool ForceFullSync = false
);

public record OAuthCallbackRequest(
    string Code,
    string State,
    string? Error = null,
    string? ErrorDescription = null
);
