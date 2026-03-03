using AI_genda_API.Contracts.Space;

namespace AI_genda_API.Services.WorkSpaceService;

public interface ISpaceService
{
   public Task<Result<IEnumerable<SpaceResponse>?>> GetAllAsync(CancellationToken cancellationToken);
}

