namespace AI_genda_API.Contracts.Tasks;

public record TaskRequest
(
 
    string Content ,
     bool IsTaskFinished = default!, 
     int  priority = default!,
     string  status = default!

);
