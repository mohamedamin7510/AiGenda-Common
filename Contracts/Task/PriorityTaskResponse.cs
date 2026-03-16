namespace AI_genda_API.Contracts.Task;

public record PriorityTaskResponse
(
     string Id,
     string Title,
     string Priority,
     DateTime? DueDate
);