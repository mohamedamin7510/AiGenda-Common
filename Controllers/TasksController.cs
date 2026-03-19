using AI_genda_API.Services.TaskService;
namespace AI_genda_API.Controllers;

[Route("api/workspaces/{WorkspaceId}/spaces/{SpaceId}/[controller]")]
[ApiController]
[Authorize]
public class TasksController(ITaskService taskService) : ControllerBase
{
    private readonly ITaskService _TaskService = taskService;

    [HttpPost]
    public async System.Threading.Tasks.Task<IActionResult> Add([FromRoute] int WorkspaceId, [FromRoute] string SpaceId, [FromBody] TaskRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _TaskService.AddAsync(WorkspaceId, SpaceId, User.GetUserId()!, request, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { WorkspaceId, SpaceId, Id = result.Value.Id }, result.Value) : result.ToProblem();
    }

    [HttpGet]
    public async System.Threading.Tasks.Task<IActionResult> GetAll([FromRoute] int WorkspaceId, [FromRoute] string SpaceId, CancellationToken cancellationToken = default!)
    {
        var result = await _TaskService.GetAllAsync(WorkspaceId, SpaceId, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{Id}")]
    public async System.Threading.Tasks.Task<IActionResult> GetById([FromRoute] int WorkspaceId, [FromRoute] string SpaceId, [FromRoute] string Id, CancellationToken cancellationToken = default!)
    {
        var result = await _TaskService.GetByIdAsync(WorkspaceId, SpaceId, Id, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{Id}")]
    public async System.Threading.Tasks.Task<IActionResult> Update([FromRoute] int WorkspaceId, [FromRoute] string SpaceId, [FromRoute] string Id, [FromBody] TaskRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _TaskService.UpdateAsync(WorkspaceId, SpaceId, Id, User.GetUserId()!, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{Id}/status")]
    public async System.Threading.Tasks.Task<IActionResult> UpdateStatus([FromRoute] int WorkspaceId, [FromRoute] string SpaceId, [FromRoute] string Id, [FromBody] UpdateTaskStatusRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _TaskService.UpdateStatusAsync(WorkspaceId, SpaceId, Id, User.GetUserId()!, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpDelete("{Id}")]
    public async System.Threading.Tasks.Task<IActionResult> Delete([FromRoute] int WorkspaceId, [FromRoute] string SpaceId, [FromRoute] string Id, CancellationToken cancellationToken = default!)
    {
        var result = await _TaskService.DeleteAsync(WorkspaceId, SpaceId, Id, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPut("{Id}/restore")]
    public async System.Threading.Tasks.Task<IActionResult> Restore([FromRoute] int WorkspaceId, [FromRoute] string SpaceId, [FromRoute] string Id, CancellationToken cancellationToken = default!)
    {
        var result = await _TaskService.RestoreAsync(WorkspaceId, SpaceId, Id, User.GetUserId()!, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPut("{Id}/assign")]
    public async System.Threading.Tasks.Task<IActionResult> AssignMember([FromRoute] int WorkspaceId, [FromRoute] string SpaceId, [FromRoute] string Id, [FromBody] AssignTaskRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _TaskService.AssignMemberAsync(WorkspaceId, SpaceId, Id, User.GetUserId()!, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpDelete("{Id}/unassign")]
    public async System.Threading.Tasks.Task<IActionResult> UnAssignMember([FromRoute] int WorkspaceId, [FromRoute] string SpaceId, [FromRoute] string Id, [FromBody] AssignTaskRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _TaskService.UnAssignMemberAsync(WorkspaceId, SpaceId, Id, User.GetUserId()!, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}