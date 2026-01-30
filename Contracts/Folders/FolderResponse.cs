namespace AI_genda_API.Contracts.Folders;

public record FolderResponse
(
    int Id,
    string Name , 
    List<FolderResponse> ChilFolders,
    List<TaskResponse> Tasks ,
    List<NoteResponse> Notes

);
