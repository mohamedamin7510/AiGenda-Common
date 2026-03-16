using AI_genda_API.Contracts.Workspace;

namespace AI_genda_API.Services.FolderService;

public interface IWorkSpaceService
{
    public Task<Result<WorkSpaceResponse>> AddAsync(string UserId, WorkSpaceRequest Requset, CancellationToken cancellationToken = default!);   
    public Task<Result<IEnumerable<WorkSpaceResponse>?>> GetAllAsync( CancellationToken cancellationToken = default!);
    public Task<Result<WorkSpaceResponse>> GetByIdAsync(int id, string? userId, CancellationToken cancelationToken);
    public Task<Result<WorkspaceDashboardResponse>> GetWorkspaceDashboardAsync(int Id , string UserId,  CancellationToken cancellationToken);
    public Task <Result<WorkSpaceResponse>> UpdateAsync(int Id, string UserId,  WorkSpaceRequest requset, CancellationToken cancellationToken);
    public Task<Result> DeleteAsync (int Id, CancellationToken cancellationToken);
    public Task<Result> RestoreAsync(int Id, CancellationToken cancellationToken);
    public Task<Result> AddMemberAsync(int Id ,string InviterUserId, InviteMember request, CancellationToken cancellationToken);
    public Task<Result> RemoveMemberAsync(int Id , string RemovedUserId ,InviteMember request ,CancellationToken cancellationToken = default!);

     
   
}
