namespace AI_genda_API.Authentication;

public interface IJWTProvider
{
    public (string Token, int Expiresin) GenerateToken(ExtendedUser extendedUser, IEnumerable<string> roles, IEnumerable<string> permissions, int? customExpirationMinutes = null);
    public string? ValidateToken(string token);
}
