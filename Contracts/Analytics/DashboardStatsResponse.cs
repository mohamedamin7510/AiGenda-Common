namespace AI_genda_API.Contracts.Analytics;

public record DashboardStatsResponse
(
     int TotalTasks,
     double FocusTimeHours,
     int ActiveSpaces,
     int ProductivityScore
);