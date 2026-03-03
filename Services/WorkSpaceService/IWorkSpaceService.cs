

using AI_genda_API.Contracts.Workspace;

namespace AI_genda_API.Services.FolderService;

public interface IWorkSpaceService
{
    public Task<Result<IEnumerable<WorkSpaceResponse>?>> GetAllAsync( CancellationToken cancellationToken = default!);
    public Task<Result<WorkSpaceResponse>> AddAsync(WorkSpaceRequest Requset, CancellationToken cancellationToken = default!);    
    public Task<Result> DeleteAsync (int id, CancellationToken cancellationToken);
    //public Task<Result> UpdateAsync(int Id, FolderRequset folderRequset, CancellationToken cancellationToken);

}
