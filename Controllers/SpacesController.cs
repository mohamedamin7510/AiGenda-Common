using AI_genda_API.Services.WorkSpaceService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AI_genda_API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SpacesController(ISpaceService spaceService) : ControllerBase
{
    private readonly ISpaceService _SpaceService = spaceService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default!)
    {
         var result = await _SpaceService.GetAllAsync(cancellationToken);

        return Ok(result.Value);
    }



}
