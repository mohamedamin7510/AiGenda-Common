using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Abstractions.Errors;

namespace AI_genda_API.Services.TokenManagement;

public interface ITokenManagerService
{
    /// <summary>
    /// Authenticates transparently for the AI Agent proxy. Retrieves a decrypted, valid Access Token 
    /// for the specified user and provider. Automatically handles token refresh logic if expired.
    /// </summary>
    Task<Result<string>> GetValidAccessTokenAsync(string userId, AppProvider provider, CancellationToken cancellationToken = default);
}
