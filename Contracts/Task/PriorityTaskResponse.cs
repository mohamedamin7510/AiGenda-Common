using AI_genda_API.Entities.Enums;
using TaskStatus = AI_genda_API.Entities.Enums.TaskStatuss;
using TaskPriority = AI_genda_API.Entities.Enums.TaskPriority;

namespace AI_genda_API.Contracts.Task;

public record TaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate
);

public record UpdateTaskStatusRequest(
    TaskStatus Status
);

public record AssignTaskRequest(
    string Email
);

public record AssigneeResponse(
    string UserId,
    string FullName,
    string? AvatarUrl
);

public record TaskResponse(
    string Id,
    string Title,
    string? Description,
    TaskStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    DateTime CreatedAt,
    List<AssigneeResponse> Assignees
);