using AI_genda_API.Contracts.ProfileSetting;

namespace AI_genda_API.Services.ProfileSettingService;

public interface IProfileSettingService
{
   Task<Result<ProfileResponse>> GetAsync(string Id);

}
