using AI_genda_API.Abstractions.Enums;

namespace AI_genda_API.Entities;

public class AppConnection : AuditLogging
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string UserId { get; set; } = string.Empty;
    
    public int? WorkSpaceId { get; set; }
    
    public AppProvider Provider { get; set; }
    
    /// <summary>
    /// External account identifier (e.g., GitHub username, Google email, etc.)
    /// </summary>
    public string ExternalAccountId { get; set; } = string.Empty;
    
    /// <summary>
    /// Encrypted OAuth access token or API key
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Encrypted OAuth refresh token (nullable for non-OAuth flows)
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// When the access token expires (UTC)
    /// </summary>
    public DateTime? TokenExpiresAt { get; set; }
    
    /// <summary>
    /// Whether this connection is active and should be synced
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// How frequently to sync data from this connection
    /// </summary>
    public SyncFrequency SyncFrequency { get; set; } = SyncFrequency.Daily;
    
    /// <summary>
    /// When the last successful sync occurred
    /// </summary>
    public DateTime? LastSyncAt { get; set; }
    
    /// <summary>
    /// Current sync status
    /// </summary>
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
    
    /// <summary>
    /// Message message from the last failed sync attempt
    /// </summary>
    public string? LastSyncError { get; set; }
    
    /// <summary>
    /// Provider-specific settings stored as JSON
    /// Example: {"scopes": "calendar,profile", "syncEvents": true, "eventTypes": ["meeting", "task"]}
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Scopes/permissions granted by the user for this connection
    /// </summary>
    public string? GrantedScopes { get; set; }
    
    // Navigation properties
    public virtual ExtendedUser User { get; set; } = null!;
    public virtual WorkSpace? WorkSpace { get; set; }
}
