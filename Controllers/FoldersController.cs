using AI_genda_API.Contracts.Folders;
using AI_genda_API.Services.FolderService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Threading;

namespace AI_genda_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FoldersController(IFolderService folderService) : ControllerBase
{
    private readonly IFolderService _FolderService = folderService;

    // todo: Add user then Test the AuditLogging .

    [HttpGet]
    public async Task<IActionResult> GetAllFolders(CancellationToken cancellationToken = default!)
    {
       var result = await _FolderService.GetAllFoldersAsync( cancellationToken );

        return Ok(result!);
    }
    [HttpGet("{Id}")]
    public async Task<IActionResult> GetFolderById(CancellationToken cancellationToken = default!)
    {
        //todo: You shoul implemnt this controller with loading the children for other 

        return Ok(1);
    }

    [HttpPost]
    public async Task<IActionResult> AddFolder(FolderRequset folderRequset, CancellationToken cancellationToken = default!)
    {
        var response = await _FolderService.AddFolderAsync(folderRequset ,cancellationToken);
        return CreatedAtAction(nameof(GetFolderById), new {Id= response.Id }, response);
    }
    [HttpDelete("{Id}")]
    public async Task<IActionResult> DeleteFolder([FromRoute] int Id ,  CancellationToken cancellationToken = default!)
    {

        var res = await  _FolderService.DeleteFolderAsync (Id ,cancellationToken);
        return res ? NoContent() : BadRequest();
    }

    [HttpPut("{Id}")]
    public async Task<IActionResult> UpdateFolder([FromRoute] int Id ,FolderRequset folderRequset,   CancellationToken cancellationToken = default!)
    {

        var res = await  _FolderService.UpdateFolderAsync (Id , folderRequset , cancellationToken);
        return res ? NoContent() : BadRequest();
    }




}
