using AI_genda_API.Abstractions.Const;
using AI_genda_API.Abstractions.Filters;
using AI_genda_API.Contracts.Space;

namespace AI_genda_API.Controllers;

[ApiController]
[Route("api/workspaces/{WorkspaceId:int}/[controller]")]
[Authorize(Roles = DefaultRoles.Member)]
public class SpacesController(ISpaceService spaceService) : ControllerBase
{
    private readonly ISpaceService _SpaceService = spaceService;

    [HttpPost]
    [HasPermission(Permissions.AddSpaces)]
    public async Task<IActionResult> Add([FromRoute] int WorkspaceId, [FromBody] SpaceRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _SpaceService.AddAsync(WorkspaceId, User.GetUserId()!, request, cancellationToken);

        return result.IsSuccess ?
            CreatedAtAction(nameof(GetById), new { WorkspaceId, Id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    [HttpGet]
    [HasPermission(Permissions.GetSpaces)]
    public async Task<IActionResult> GetAll([FromRoute] int WorkspaceId, CancellationToken cancellationToken = default!)
    {
        var result = await _SpaceService.GetAllAsync(WorkspaceId, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("deleted")]
    [HasPermission(Permissions.DeleteSpaces)]
    public async Task<IActionResult> GetAllDeleted([FromRoute] int WorkspaceId, CancellationToken cancellationToken = default!)
    {
        var result = await _SpaceService.GetAllDeletedAsync(WorkspaceId, User.GetUserId()!, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{Id}")]
    [HasPermission(Permissions.GetSpaces)]
    public async Task<IActionResult> GetById([FromRoute] int WorkspaceId, [FromRoute] string Id, CancellationToken cancellationToken = default!)
    {
        var result = await _SpaceService.GetByIdAsync(WorkspaceId, Id, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

   
    [HttpPut("{Id}")]
    [HasPermission(Permissions.UpdateSpaces)]
    public async Task<IActionResult> Update([FromRoute] int WorkspaceId, [FromRoute] string Id, [FromBody] SpaceRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _SpaceService.UpdateAsync(WorkspaceId, Id, User.GetUserId()!, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{Id}/move")]
    [HasPermission(Permissions.UpdateSpaces)]
    public async Task<IActionResult> Move([FromRoute] int WorkspaceId, [FromRoute] string Id, [FromBody] MoveSpaceRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _SpaceService.MoveAsync(WorkspaceId, Id, User.GetUserId()!, request, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpDelete("{Id}")]
    [HasPermission(Permissions.DeleteSpaces)]
    public async Task<IActionResult> Delete([FromRoute] int WorkspaceId, [FromRoute] string Id, CancellationToken cancellationToken = default!)
    {
        var result = await _SpaceService.DeleteAsync(WorkspaceId, Id, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPut("{Id}/restore")]
    [HasPermission(Permissions.DeleteSpaces)]
    public async Task<IActionResult> Restore([FromRoute] int WorkspaceId, [FromRoute] string Id, CancellationToken cancellationToken = default!)
    {
        var result = await _SpaceService.RestoreAsync(WorkspaceId, Id, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}