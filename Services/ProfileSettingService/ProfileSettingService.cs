using AI_genda_API.Contracts.ProfileSetting;
using BucketSurvey.Api.Contract.User;

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

    public async Task<Result<ProfileResponse>> UpdateAsync(string Id, ProfileRequest request, CancellationToken cancellationToken)
    {

        var user = await _UserManager.FindByIdAsync(Id);

        if (user is null || user.IsDeleted)
            return Result.Faluire<ProfileResponse>(UserErrors.UserIsDeleted);

        user.FirstName = request.FirstName;
        user.SecondName = request.SecondName;
        user.DateOfBirth = user.DateOfBirth;
        user.JobTitle = request.JobTitle;

        await _UserManager.UpdateAsync(user);

        var response = user.Adapt<ProfileResponse>();

        return Result.Success(response);

    }

    public async Task<Result> ChangePasswordAsync(string Id, ChangePasswordRequest request)
    {
        var user = await _UserManager.FindByIdAsync(Id);

        var result = await _UserManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
        {
            return Result.Success();
        }

        var error = result.Errors.First();

        return Result.Faluire(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }


}
