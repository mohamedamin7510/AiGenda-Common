public record SpaceDetailResponse(
    string Id,
    string Name,
    string? Description,
    string IconHexa,
    DateOnly LastActivity,
    bool IsPublic,
    int TotalTasks,
    int CompletedTasks
);
