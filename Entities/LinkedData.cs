using AI_genda_API.Abstractions.Enums;

namespace AI_genda_API.Entities;

/// <summary>
/// Stores raw synced data from external providers for the AI agent to consume
/// </summary>
public class LinkedData : AuditLogging
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string UserId { get; set; } = string.Empty;
    
    public int? WorkSpaceId { get; set; }
    
    public string AppConnectionId { get; set; } = string.Empty;
    
    public AppProvider Provider { get; set; }
    
    /// <summary>
    /// Type of data: Event, Activity, Issue, Commit, etc.
    /// </summary>
    public string DataType { get; set; } = string.Empty;
    
    /// <summary>
    /// External identifier from the provider (e.g., calendar event ID, commit SHA)
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;
    
    /// <summary>
    /// Raw data from external API stored as JSON
    /// </summary>
    public string RawData { get; set; } = string.Empty;
    
    /// <summary>
    /// When this data was synced
    /// </summary>
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Summary or normalized title for quick reference
    /// </summary>
    public string? Summary { get; set; }
    
    /// <summary>
    /// Optional start time (for calendar events, focus sessions, etc.)
    /// </summary>
    public DateTime? StartTime { get; set; }
    
    /// <summary>
    /// Optional end time
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    // Navigation properties
    public virtual ExtendedUser User { get; set; } = null!;
    public virtual WorkSpace? WorkSpace { get; set; }
    public virtual AppConnection AppConnection { get; set; } = null!;
}
