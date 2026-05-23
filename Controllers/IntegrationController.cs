using AI_genda_API.Contracts.AppConnections;
using Microsoft.AspNetCore.Mvc;

namespace AI_genda_API.Controllers;

[ApiController]
[Route("integrations/v1/[controller]")]
public abstract class IntegrationController : ControllerBase
{
    protected IActionResult IntegrationSuccess<T>(T data)
    {
        return Ok(IntegrationResponse<T>.Success(data));
    }
}
