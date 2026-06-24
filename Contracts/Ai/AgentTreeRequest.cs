namespace AI_genda_API.Contracts.Ai;

public record AgentTreeRequest(AgentWorkspace Workspace);

public record AgentWorkspace(
    string Name,
    string? Description,
    List<AgentSpace> Spaces
);

public record AgentSpace(
    string Name,
    string? Description,
    string? IconCode,
    List<AgentTask> Tasks
);

public record AgentTask(
    string Title,
    string? Description,
    DateTime? DueDate,
    List<AgentSubTask> Subtasks
);

public record AgentSubTask(
    string Title,
    bool IsCompleted
);
