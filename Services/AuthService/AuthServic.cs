using System.Security.Cryptography;
using System.Text;

namespace AI_genda_API.Services.AuthService;

public class AuthServic(UserManager<ExtendedUser> userManager
    , IJWTProvider jWTProvider) : IAuthService

{

    private UserManager<ExtendedUser> _UserManager { get; } = userManager;
    private readonly IJWTProvider _JWTProvider = jWTProvider;
    private readonly int _RefreshTokenExpiryDate = 14; 

    public async Task<AuthResponse?> GetTokenAsync(string email, string Pass, CancellationToken? cancellationToken = default)
    {

        var user = await _UserManager.FindByEmailAsync(email);
        if (user is null)
            return null;


        bool res = await _UserManager.CheckPasswordAsync(user, Pass);
        if (!res)
            return null;
      
        var GeneratedTokenInfo = _JWTProvider.GenerateToken(user);

        var GeneratedrefreshToken = GenerateRefreshToken();
        var ReftokenExpiryDate = DateTime.UtcNow.AddDays(_RefreshTokenExpiryDate);
        user.RefreshTokens.Add(new RefreshToken() { refreshToken = GeneratedrefreshToken, ExpiredAt = ReftokenExpiryDate });

        await _UserManager.UpdateAsync(user);


        return new AuthResponse(user.Id,
            FirstName: user.FirstName!,
            SecondName: user.SecondName!,
            Email: user.Email!,
            Token: GeneratedTokenInfo.Token,
            ExpiredIn: GeneratedTokenInfo.Expiresin * 60,
            RefreshToken: GeneratedrefreshToken,
            ExpiryDate: ReftokenExpiryDate
            );




    }

    public async Task<AuthResponse?> GetRefreshTokenAsync(string refreshtoken, string token, CancellationToken? cancellationToken = null)
    {
        string? userid = _JWTProvider.ValidateToken(token);
        if (userid is null) return null;

        var user = await _UserManager.FindByIdAsync(userid);
        if (user is null) return null;

        var reftoken = user.RefreshTokens.FirstOrDefault(x => x.refreshToken == refreshtoken && x.IsActive);
        if (reftoken is null) return null;
        reftoken.RevokedAt = DateTime.UtcNow;

       await _UserManager.UpdateAsync(user);
        
        var GeneratedTokenInfo = _JWTProvider.GenerateToken(user);
        var GeneratedrefreshToken = GenerateRefreshToken();
        var ReftokenExpiryDate = DateTime.UtcNow.AddDays(_RefreshTokenExpiryDate);

        user.RefreshTokens.Add(new RefreshToken() { refreshToken = GeneratedrefreshToken, ExpiredAt = ReftokenExpiryDate });

        await _UserManager.UpdateAsync(user);


        return new AuthResponse(user.Id,
            FirstName: user.FirstName!,
            SecondName: user.SecondName!,
            Email: user.Email!,
            Token: GeneratedTokenInfo.Token,
            ExpiredIn: GeneratedTokenInfo.Expiresin * 60,
            RefreshToken: GeneratedrefreshToken,
            ExpiryDate: ReftokenExpiryDate
            );



    }



    public async Task<bool> RevokeRefreshTokenAsync(string refreshtoken, string token, CancellationToken? cancellationToken = default)
    {

        string? userid = _JWTProvider.ValidateToken(token);
        if (userid is null) return false;

        var user = await _UserManager.FindByIdAsync(userid);
        if (user is null) return false;

        var reftoken = user.RefreshTokens.FirstOrDefault(x => x.refreshToken == refreshtoken && x.IsActive);
        if (reftoken is null) return false;

        reftoken.RevokedAt = DateTime.UtcNow;

       await  _UserManager.UpdateAsync(user);

        return true; 
    }
    private string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

}
