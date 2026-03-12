using AI_genda_API.Contracts.ProfileSetting;

namespace AI_genda_API.Services.ProfileSettingService;

public class ProfileSettingService(UserManager<ExtendedUser> userManager) : IProfileSettingService
{
    private readonly UserManager<ExtendedUser> _UserManager = userManager;

    public async Task<Result<ProfileResponse>> GetAsync(string Id)
    {
        var user = await _UserManager.FindByIdAsync(Id);

        if (user is null || user.IsDeleted)
            return Result.Faluire<ProfileResponse>(UserErrors.UserIsDeleted);

        var response = user.Adapt<ExtendedUser, ProfileResponse>();

        return Result.Success(response);
    }
}
