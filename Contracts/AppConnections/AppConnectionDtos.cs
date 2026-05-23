using System.Text.Json.Serialization;
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
    string? Metadata,
    DateTime? TokenExpiresAt
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

// Note: the properties are strictly lower case naming to match expected payload mapping if required by default or use JSON property names.
public record IntegrationsStatusResponse(
    [property: JsonPropertyName("github")] bool Github,
    [property: JsonPropertyName("gmail")] bool Gmail,
    [property: JsonPropertyName("calendar")] bool Calendar
);

public record OAuthCallbackRequest(
    string Code,
    string State,
    string? Error = null,
    string? ErrorDescription = null
);
