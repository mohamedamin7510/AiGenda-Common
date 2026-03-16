using AI_genda_API.Contracts.Profile;
using AI_genda_API.Services.ProfileSettingService;
using BucketSurvey.Api.Contract.User;

namespace AI_genda_API.Controllers;

[Route("me")]
[ApiController]
[Authorize]
public class ProfilesController(IProfileService profileSettingService, 
    UserManager<ExtendedUser> userManager) : ControllerBase
{
    private readonly IProfileService _ProfileService = profileSettingService;
    private readonly UserManager<ExtendedUser> _UserManager = userManager;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        
        var result = await _ProfileService.GetAsync(User.GetUserId()!);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem(); 
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ProfileRequest request,CancellationToken cancellationToken )
    {
        var result = await _ProfileService.UpdateAsync(User.GetUserId()!, request , cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();

    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _ProfileService.ChangePasswordAsync(User.GetUserId()!, request);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }


    [HttpPost("change-email")]//send code to new email
    public async Task<IActionResult> ConfirmChangeEmail([FromBody] changeEmailRequest request)
    {
        var result = await _ProfileService.ChangeEmailAsync(User.GetUserId()!,request);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }


    [HttpPut("confirm-change-email")]// verify code and change email
    public async Task<IActionResult> ChangeEmail([FromBody] ConfirmChangeEmailRequest request)
    {
        var result = await _ProfileService.ConfirmChangeEmailAsync(request);

        return result.IsSuccess ? NoContent() : result.ToProblem();

    }




    //todo: POST.. /me/avatar
    //todo: DELETE../me/avatar

}
