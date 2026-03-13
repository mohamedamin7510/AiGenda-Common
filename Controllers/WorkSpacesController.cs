using AI_genda_API.Contracts.Workspace;
using AI_genda_API.Services.FolderService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Threading;

namespace AI_genda_API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class WorkSpacesController(IWorkSpaceService workSpaceService) : ControllerBase
{
    private readonly IWorkSpaceService _WorkSpaceService = workSpaceService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default!)
    {
        var result = await _WorkSpaceService.GetAllAsync(cancellationToken);

        return Ok(result!.Value);
    }


    [HttpGet("{Id}")]
    public async Task<IActionResult> GetById(CancellationToken cancellationToken = default!)
    {
        // todo: You shoul implemnt this controller with loading the children for other

        return Ok(1);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] WorkSpaceRequest Requset,
        CancellationToken cancellationToken = default!)
    {
        var response = await _WorkSpaceService.AddAsync(Requset, cancellationToken);

        return CreatedAtAction(nameof(GetById),
            new { Id = response.Value.Id }, response.Value);
    }

    [HttpPut("{Id}")]
    public async Task<IActionResult> Update([FromRoute] int Id, [FromBody] WorkSpaceRequest rquset,
        CancellationToken cancellationToken = default!)
    {
        var res = await _WorkSpaceService.UpdateAsync(User.GetUserId()!, Id, rquset, cancellationToken);

        return res.IsSuccess ? NoContent() : res.ToProblem();
    }

    [HttpDelete("{Id}")]
    public async Task<IActionResult> Delete([FromRoute] int Id, CancellationToken cancellationToken = default!)
    {
        var res = await _WorkSpaceService.DeleteAsync(Id, cancellationToken);

        return res.IsSuccess ? NoContent() :
         res.ToProblem();
    }


    [HttpPost("{Id}/member")]
    public async Task<IActionResult> AddMember ([FromRoute] int Id,InviteMemberRequest request,
        CancellationToken cancellationToken = default!)
    {
        var res = await _WorkSpaceService.AddMemberAsync(Id,User.GetUserId()!,request ,  cancellationToken);

        return res.IsSuccess ? NoContent() : res.ToProblem();
    }



}
