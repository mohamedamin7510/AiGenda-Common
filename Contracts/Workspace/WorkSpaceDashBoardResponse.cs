using AI_genda_API.Contracts.Activity;
using AI_genda_API.Contracts.Analytics;
using AI_genda_API.Contracts.Focus;
using AI_genda_API.Contracts.Space;
using AI_genda_API.Contracts.Task;

namespace AI_genda_API.Contracts.Workspace;

public record WorkspaceDashboardResponse
(
     DashboardStatsResponse Stats ,
         WeeklyFocusTimeResponse WeeklyFocusTime ,
             List<ActivityResponse> RecentActivities ,
                 List<PriorityTaskResponse> PriorityTasks, 
                     List<SpaceResponse> Spaces
);
