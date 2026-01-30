namespace AI_genda_API.Services.AuthService;

public interface IAuthService
{
   public  Task<AuthResponse?> GetTokenAsync(string email, string Pass, CancellationToken? cancellationToken = default);
   public  Task<AuthResponse?> GetRefreshTokenAsync(string refreshtoken, string token, CancellationToken? cancellationToken = default);
   public  Task< bool> RevokeRefreshTokenAsync(string refreshtoken, string token, CancellationToken? cancellationToken = default);

}
