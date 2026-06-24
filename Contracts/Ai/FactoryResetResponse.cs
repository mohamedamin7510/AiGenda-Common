namespace AI_genda_API.Contracts.Ai;

public record FactoryResetResponse(
    int WorkspacesDeleted,
    int SpacesDeleted,
    int TasksDeleted,
    int SubTasksDeleted,
    int WorkspaceMembersDeleted
);
