namespace AI_genda_API.Services.SpaceService;

public interface ISpaceService
{
    Task<Result<SpaceDetailResponse>> AddAsync(int WorkspaceId, string UserId, SpaceRequest request, CancellationToken cancellationToken = default!);
    Task<Result<IEnumerable<SpaceDetailResponse>>> GetAllAsync(int WorkspaceId, string UserId, CancellationToken cancellationToken = default!);
    Task<Result<SpaceDetailResponse>> GetByIdAsync(int WorkspaceId, string Id, string UserId, CancellationToken cancellationToken = default!);
    Task<Result<SpaceDetailResponse>> UpdateAsync(int WorkspaceId, string Id, string UserId, SpaceRequest request, CancellationToken cancellationToken = default!);
    Task<Result> DeleteAsync(int WorkspaceId, string Id, string UserId, CancellationToken cancellationToken = default!);
    Task<Result> RestoreAsync(int WorkspaceId, string Id, string UserId, CancellationToken cancellationToken = default!);
}