using AI_genda_API.Contracts.ProfileSetting;
using BucketSurvey.Api.Contract.User;

namespace AI_genda_API.Services.ProfileSettingService;

public interface IProfileSettingService
{
   Task<Result<ProfileResponse>> GetAsync(string Id);
   Task<Result<ProfileResponse>> UpdateAsync(string Id , ProfileRequest request , CancellationToken cancellationToken);
   Task<Result> ChangePasswordAsync(string Id, ChangePasswordRequest request);



}
