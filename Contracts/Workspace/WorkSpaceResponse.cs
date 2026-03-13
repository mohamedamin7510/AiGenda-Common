namespace AI_genda_API.Contracts.Workspace;

public record WorkSpaceResponse
(
    int Id,
    string Name , 
    string Description, 
    string IconCode , 
    int NumberofMembers ,
    int NumberofTasks
);