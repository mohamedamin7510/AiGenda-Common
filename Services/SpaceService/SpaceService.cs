using AI_genda_API.Contracts.Space;

namespace AI_genda_API.Services.SpaceService;

public class SpaceService(AppContext context) : ISpaceService
{
    private readonly AppContext _Context = context;

    public async Task<Result<IEnumerable<SpaceResponse>?>> GetAllAsync(CancellationToken cancellationToken)
    {
        var Spaces = await  _Context.Spaces
                .Where(x => x.IsActive).AsNoTracking()
                .ToListAsync(cancellationToken);

        var response = Spaces.Adapt<IEnumerable<SpaceResponse>>();

        return Result.Success(response)!;
    }
}
