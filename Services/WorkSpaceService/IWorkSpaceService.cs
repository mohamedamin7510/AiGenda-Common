using AI_genda_API.Contracts.Workspace;

namespace AI_genda_API.Services.FolderService;

public interface IWorkSpaceService
{
    public Task<Result<IEnumerable<WorkSpaceResponse>?>> GetAllAsync( CancellationToken cancellationToken = default!);
    public Task<Result<WorkSpaceResponse>> AddAsync(WorkSpaceRequest Requset, CancellationToken cancellationToken = default!);    
    public Task<Result> DeleteAsync (int id, CancellationToken cancellationToken);
    public Task<Result> UpdateAsync(string userid , int id, WorkSpaceRequest requset, CancellationToken cancellationToken);
    public Task<Result> AddMemberAsync(int WorkSpaceId ,string UserId, InviteMemberRequest request, CancellationToken cancellationToken);
    // public Task<Result> RemoveMemberAsync(string v, InviteMemberRequest request, CancellationToken cancellationToken);
}
