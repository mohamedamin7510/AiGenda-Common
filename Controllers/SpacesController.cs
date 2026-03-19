using AI_genda_API.Contracts.Space;
using AI_genda_API.Services.SpaceService;

namespace AI_genda_API.Controllers;

[Route("api/workspaces/{WorkspaceId}/[controller]")]
[ApiController]
[Authorize]
public class SpacesController(ISpaceService spaceService) : ControllerBase
{
    private readonly ISpaceService _SpaceService = spaceService;

    [HttpPost]
    public async System.Threading.Tasks.Task<IActionResult> Add([FromRoute] int WorkspaceId, [FromBody] SpaceRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _SpaceService.AddAsync(WorkspaceId, User.GetUserId()!, request, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { WorkspaceId, Id = result.Value.Id }, result.Value) : result.ToProblem();
    }

    [HttpGet]
    public async System.Threading.Tasks.Task<IActionResult> GetAll([FromRoute] int WorkspaceId, CancellationToken cancellationToken = default!)
    {
        var result = await _SpaceService.GetAllAsync(WorkspaceId, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{Id}")]
    public async System.Threading.Tasks.Task<IActionResult> GetById([FromRoute] int WorkspaceId, [FromRoute] string Id, CancellationToken cancellationToken = default!)
    {
        var result = await _SpaceService.GetByIdAsync(WorkspaceId, Id, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{Id}")]
    public async System.Threading.Tasks.Task<IActionResult> Update([FromRoute] int WorkspaceId, [FromRoute] string Id, [FromBody] SpaceRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _SpaceService.UpdateAsync(WorkspaceId, Id, User.GetUserId()!, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpDelete("{Id}")]
    public async System.Threading.Tasks.Task<IActionResult> Delete([FromRoute] int WorkspaceId, [FromRoute] string Id, CancellationToken cancellationToken = default!)
    {
        var result = await _SpaceService.DeleteAsync(WorkspaceId, Id, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPut("{Id}/restore")]
    public async System.Threading.Tasks.Task<IActionResult> Restore([FromRoute] int WorkspaceId, [FromRoute] string Id, CancellationToken cancellationToken = default!)
    {
        var result = await _SpaceService.RestoreAsync(WorkspaceId, Id, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}