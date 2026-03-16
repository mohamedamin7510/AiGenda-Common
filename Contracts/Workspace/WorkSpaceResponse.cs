using AI_genda_API.Entities.Enums;

namespace AI_genda_API.Contracts.Workspace;

public record WorkSpaceResponse
(
    int Id,
    string Name , 
    string Description, 
    string IconCode, 
    WorkSpaceVisibility Visibility,
    int NumberofMembers ,
    int NumberofTasks
);