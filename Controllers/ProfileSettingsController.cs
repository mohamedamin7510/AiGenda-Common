using AI_genda_API.Contracts.ProfileSetting;
using AI_genda_API.Services.ProfileSettingService;

namespace AI_genda_API.Controllers;

[Route("me")]
[ApiController]
public class ProfileSettingsController(IProfileSettingService profileSettingService) : ControllerBase
{
    private readonly IProfileSettingService _ProfileSettingService = profileSettingService;

    [HttpGet]
    public async Task<IActionResult> Get(string userId)
    {
        var result = await _ProfileSettingService.GetAsync(userId);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem(); 
    }

/*    public async Task<ServiceResult<UserDto>> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.IsDeleted)
            return ServiceResult<UserDto>.Fail("User not found.");

        user.FullName = dto.FullName;
        user.Bio = dto.Bio;
        user.Timezone = dto.Timezone;
        user.AvatarUrl = dto.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        return ServiceResult<UserDto>.Ok(user.Adapt<UserDto>());
    }*/


}
