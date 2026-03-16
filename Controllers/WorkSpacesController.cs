using AI_genda_API.Contracts.Workspace;

namespace AI_genda_API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class WorkSpacesController(IWorkSpaceService workSpaceService) : ControllerBase
{
    private readonly IWorkSpaceService _WorkSpaceService = workSpaceService;


    #region Basic CRUD Operations 

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] WorkSpaceRequest Requset, CancellationToken cancellationToken = default!)
    {
        var response = await _WorkSpaceService.AddAsync(User.GetUserId()!, Requset, cancellationToken);

        return CreatedAtAction(nameof(GetById),
            new { Id = response.Value.Id }, response.Value);
    }


    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default!)
    {
        var result = await _WorkSpaceService.GetAllAsync(cancellationToken);

        return Ok(result!.Value);
    }


    [HttpGet("{Id}")]
    public async Task<IActionResult> GetById([FromRoute]int id , CancellationToken CancelationToken)
    {
        var userId = User.GetUserId();

        var result = await _WorkSpaceService.GetByIdAsync(id, userId, CancelationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();


     }

  

    [HttpGet("{Id}/dashboard")]
    public async Task<IActionResult> GetWorkspaceDashboardData([FromRoute] int Id, CancellationToken cancellationToken = default!)
    {
        var userId = User.GetUserId();

        var result = await _WorkSpaceService.GetWorkspaceDashboardAsync(Id, userId!, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    

    [HttpPut("{Id}")]
    public async Task<IActionResult> Update([FromRoute] int Id, [FromBody] WorkSpaceRequest rquset,
        CancellationToken cancellationToken = default!)
    {
        var result = await _WorkSpaceService.UpdateAsync( Id ,User.GetUserId()!,rquset, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpDelete("{Id}")]
    public async Task<IActionResult> Delete([FromRoute] int Id, CancellationToken cancellationToken = default!)
    {
        var res = await _WorkSpaceService.DeleteAsync(Id, cancellationToken);

        return res.IsSuccess ? NoContent() : res.ToProblem();
    }

    [HttpPut("{Id}/restore")]
    public async Task<IActionResult> Restore([FromRoute] int Id, CancellationToken cancellationToken = default!)
    {
        var result = await _WorkSpaceService.RestoreAsync(Id, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }



    #endregion

    [HttpPost("{Id}/member")]
    public async Task<IActionResult> AddMember([FromRoute] int Id, InviteMember request,CancellationToken cancellationToken = default!)
    {
        var res = await _WorkSpaceService.AddMemberAsync(Id, User.GetUserId()!, request, cancellationToken);

        return res.IsSuccess ? NoContent() : res.ToProblem();
    }


    [HttpDelete("{Id}/remove")]
    public async Task<IActionResult> RemoveMember([FromRoute] int Id, InviteMember request, CancellationToken cancellationToken = default!)
    {
        var res = await _WorkSpaceService.RemoveMemberAsync( Id , User.GetUserId()!, request, cancellationToken);

        return res.IsSuccess ? NoContent() : res.ToProblem();
    }





}
