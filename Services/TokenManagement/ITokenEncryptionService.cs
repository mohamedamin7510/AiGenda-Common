namespace AI_genda_API.Services.TokenManagement;

public interface ITokenEncryptionService
{
    string EncryptToken(string plainToken);
    string DecryptToken(string encryptedToken);
}
