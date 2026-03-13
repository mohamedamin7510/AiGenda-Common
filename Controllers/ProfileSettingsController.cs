using AI_genda_API.Services.ProfileSettingService;
using BucketSurvey.Api.Contract.User;

namespace AI_genda_API.Controllers;

[Route("me")]
[ApiController]
[Authorize]
public class ProfileSettingsController(IProfileSettingService profileSettingService, 
    UserManager<ExtendedUser> userManager) : ControllerBase
{
    private readonly IProfileSettingService _ProfileSettingService = profileSettingService;
    private readonly UserManager<ExtendedUser> _UserManager = userManager;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        
        var result = await _ProfileSettingService.GetAsync(User.GetUserId()!);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem(); 
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ProfileRequest request,CancellationToken cancellationToken )
    {
        var result = await _ProfileSettingService.UpdateAsync(User.GetUserId()!, request , cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();

    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _ProfileSettingService.ChangePasswordAsync(User.GetUserId()!, request);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }




}
