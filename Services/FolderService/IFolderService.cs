using AI_genda_API.Contracts.Folders;

namespace AI_genda_API.Services.FolderService;

public interface IFolderService
{
    public Task<List<FolderResponse>?> GetAllFoldersAsync( CancellationToken cancellationToken = default!);
    public Task<FolderResponse> AddFolderAsync(FolderRequset folderRequset, CancellationToken cancellationToken = default!);
    public Task<bool> DeleteFolderAsync (int id, CancellationToken cancellationToken);
    public Task<bool> UpdateFolderAsync(int Id, FolderRequset folderRequset, CancellationToken cancellationToken);

}
