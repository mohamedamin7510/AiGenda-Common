namespace AI_genda_API.Abstractions.Enums;

public enum AppProvider
{
    /// <summary>
    /// Google Calendar integration for syncing calendar events
    /// Phase 1 implementation
    /// </summary>
    Google = 0,

    // Future providers (Phase 2+):
    GitHub = 1
    // Notion = 2,
    // TickTick = 3,
    // Microsoft = 4,
    // Slack = 5
}

public enum SyncFrequency
{
    Manual = 0,
    Hourly = 1,
    Daily = 2,
    Weekly = 3
}

public enum SyncStatus
{
    Pending = 0,
    Syncing = 1,
    Success = 2,
    Failed = 3,
    Paused = 4
}
