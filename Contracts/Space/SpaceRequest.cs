namespace AI_genda_API.Contracts.Space;

public record SpaceRequest
(
    string Name , 
    string Descreption,
    string IconHexa , 
    bool IsPublic 
);
