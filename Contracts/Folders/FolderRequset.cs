namespace AI_genda_API.Contracts.Folders;

public record FolderRequset
(
    string Name ,
    int? ParentFolderId
);

