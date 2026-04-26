using Microsoft.AspNetCore.DataProtection;

namespace AI_genda_API.Services.TokenManagement;

public class TokenEncryptionService : ITokenEncryptionService
{
    private readonly IDataProtector _protector;

    public TokenEncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        // Reusing the same purpose string we established in AppConnectionService
        _protector = dataProtectionProvider.CreateProtector("AppConnection.Tokens");
    }

    public string EncryptToken(string plainToken)
    {
        if (string.IsNullOrEmpty(plainToken))
            return plainToken;

        return _protector.Protect(plainToken);
    }

    public string DecryptToken(string encryptedToken)
    {
        if (string.IsNullOrEmpty(encryptedToken))
            return encryptedToken;

        return _protector.Unprotect(encryptedToken);
    }
}
