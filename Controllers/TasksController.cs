

namespace AI_genda_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Get()
    {
        string res = "123";

        return Ok(res);
    }



    [HttpPost("")]
    public async Task<IActionResult> Add([FromBody] TaskRequest taskreq)
    {
        var task = taskreq.Adapt<task>();
        //task.Id = 2 ;
        return Ok(task.Adapt<TaskResponse>());
    }



    [HttpPatch("Toggle")]
    public async Task<IActionResult> ToggleIsTaskFinished([FromBody] int Id)
    {



        return NoContent();
    }

}