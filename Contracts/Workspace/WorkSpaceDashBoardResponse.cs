using AI_genda_API.Abstractions.Enums;

namespace AI_genda_API.Contracts.Workspace;

public record PriorityTaskResponse(
    string Id,
    string Title,
    TaskPriority Priority,
    TaskStatus Status,
    DateTime? DueDate
);

public record WorkspaceDashboardResponse(
    DashboardStatsResponse Stats,
    WeeklyFocusTimeResponse WeeklyFocusTime,
    List<ActivityResponse> RecentActivities,
    List<PriorityTaskResponse> PriorityTasks,
    List<SpaceResponse> Spaces
);