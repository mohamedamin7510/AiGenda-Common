namespace AI_genda_API.Contracts.Tasks;

public record NoteRequest
(
 
    string Content ,
     bool IsTaskFinished = default!, 
     int  priority = default!,
     string  status = default!

);
