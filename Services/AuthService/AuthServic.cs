using BucketSurvey.Api.Contract.Authentication;
using BucketSurvey.Api.Helpers;
using Hangfire;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;

namespace AI_genda_API.Services.AuthService;

public class AuthServic(
     UserManager<ExtendedUser> userManager,
     IJWTProvider jWTProvider,
     SignInManager<ExtendedUser> signInManager,
     AppContext context, 
    IEmailSender emailSender, 
     ILogger<AuthServic> logger, 
     IHttpContextAccessor httpContextAccessor
    ) : IAuthService
{

    private readonly SignInManager<ExtendedUser> _SignInManager = signInManager;
    private readonly AppContext _Context = context;
    private readonly IEmailSender _EmailSender = emailSender;
    private readonly ILogger<AuthServic> _Logger = logger;
    private readonly IHttpContextAccessor _HttpContextAccessor = httpContextAccessor;

    private UserManager<ExtendedUser> _UserManager { get; } = userManager;
    private readonly IJWTProvider _JWTProvider = jWTProvider;
    private readonly int _RefreshTokenExpiryDate = 14; 

    public async Task<Result<AuthResponse>?> GetTokenAsync( LoginReq request ,  CancellationToken? cancellationToken = default)
    {

        var user = await _UserManager.FindByEmailAsync(request.Email);

        if (user is null)
            return Result.Faluire<AuthResponse>(UserErrors.EmailnotFounded);

        var SigninResult = await _SignInManager.PasswordSignInAsync(user , request.Password , false , false);
       
        if (SigninResult.Succeeded)
        {

            var GeneratedTokenInfo = _JWTProvider.GenerateToken(user);

            var GeneratedrefreshToken = GenerateRefreshToken();

            var ReftokenExpiryDate = DateTime.UtcNow.AddDays(_RefreshTokenExpiryDate);

            user.RefreshTokens.Add(new RefreshToken() { refreshToken = GeneratedrefreshToken, ExpiredAt = ReftokenExpiryDate });

            await _UserManager.UpdateAsync(user);

            return Result.Success(new AuthResponse(user.Id,
                FirstName: user.FirstName!,
                SecondName: user.SecondName!,
                Email: user.Email!,
                Token: GeneratedTokenInfo.Token,
                ExpiredIn: GeneratedTokenInfo.Expiresin * 60,
                RefreshToken: GeneratedrefreshToken,
                ExpiryDate: ReftokenExpiryDate
                ));
        }


        return SigninResult.IsNotAllowed ? Result.Faluire<AuthResponse>(UserErrors.EmailNotConfirmed) 
            : Result.Faluire<AuthResponse>(UserErrors.Invalidcredentails);
    }

    public async Task<Result<AuthResponse>?> GetRefreshTokenAsync(ReFTokenReq request, CancellationToken? cancellationToken = null)

    {
        string? userid = _JWTProvider.ValidateToken(request.Token);

        if (userid is null)
            return Result.Faluire<AuthResponse>(UserErrors.InvalidToken);

        var user = await _UserManager.FindByIdAsync(userid);

        if (user is null) 
            return Result.Faluire<AuthResponse>(UserErrors.InvalidToken); 

        var reftoken = user.RefreshTokens.FirstOrDefault(x => x.refreshToken == request.RefreshToken && x.IsActive);

        if (reftoken is null)
            return Result.Faluire<AuthResponse>(UserErrors.InvalidToken); 

        reftoken.RevokedAt = DateTime.UtcNow;

       await _UserManager.UpdateAsync(user);
        
        var GeneratedTokenInfo = _JWTProvider.GenerateToken(user);

        var GeneratedrefreshToken = GenerateRefreshToken();

        var ReftokenExpiryDate = DateTime.UtcNow.AddDays(_RefreshTokenExpiryDate);

        user.RefreshTokens.Add(new RefreshToken() { refreshToken = GeneratedrefreshToken, ExpiredAt = ReftokenExpiryDate });

        await _UserManager.UpdateAsync(user);


        return Result.Success(
            new AuthResponse(user.Id,
            FirstName: user.FirstName!,
            SecondName: user.SecondName!,
            Email: user.Email!,
            Token: GeneratedTokenInfo.Token,
            ExpiredIn: GeneratedTokenInfo.Expiresin * 60,
            RefreshToken: GeneratedrefreshToken,
            ExpiryDate: ReftokenExpiryDate
            ));
    }
   
    public async Task<Result> RevokeRefreshTokenAsync(ReFTokenReq request, CancellationToken? cancellationToken = default)
    {

        string? userid = _JWTProvider.ValidateToken(request.Token);

        if (userid is null)
            return Result.Faluire(UserErrors.InvalidToken);

        var user = await _UserManager.FindByIdAsync(userid);

        if (user is null) 
            return Result.Faluire(UserErrors.InvalidToken);

        var reftoken = user.RefreshTokens.FirstOrDefault(x => x.refreshToken == request.RefreshToken && x.IsActive);

        if (reftoken is null) return 
                Result.Faluire(UserErrors.InvalidToken);

        reftoken.RevokedAt = DateTime.UtcNow;

        await  _UserManager.UpdateAsync(user);
 
        return Result.Success(); 
    }

    public async Task<Result<RegisterResponse>> ResgisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var IsEmailExisted = await _Context.Users.AnyAsync(x=> x.Email == request.Email );

        if (IsEmailExisted)
          return Result.Faluire<RegisterResponse>(UserErrors.EmailDuplicated);

        var NewUser = new ExtendedUser()
        {
            FirstName = request.FirstName,
            SecondName = request.LastName,
            Email = request.Email , 
            UserName = request.Email ,
            PasswordHash = request.Password
        };

        var result = await _UserManager.CreateAsync(NewUser , request.Password);

        if (result.Succeeded)
        {
            var Code = await _UserManager.GenerateEmailConfirmationTokenAsync(NewUser);

            var ReadyToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(Code));

            _Logger.LogInformation("Confirmation Code : {code}",ReadyToken);

            SendEmailConfirmation(NewUser, ReadyToken);

            var response = NewUser.Adapt<ExtendedUser, RegisterResponse>();

            return Result.Success(response); 

        }

        return Result.Faluire<RegisterResponse>
           (new Error("Register.Failed", "The Registeration is not completed ", StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {

        if (await _UserManager.FindByIdAsync(request.userId) is not { } User)
            return Result.Faluire(UserErrors.InvalidCode);

 
        if (User.EmailConfirmed)
            return Result.Faluire(UserErrors.ActiveConfirmedEmail);

        string Code = request.Code;

        try
        {
            Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
        }
        catch (FormatException)
        {
            return Result.Faluire(UserErrors.InvalidCode);
        }

        var finalresult = await _UserManager.ConfirmEmailAsync(User, Code);

        if (finalresult.Succeeded)
        {
            return Result.Success();
        }
        var error = finalresult.Errors.FirstOrDefault()!;

        return Result.Faluire(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ResendConfirmEmailAsync(ResendConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        if (await _UserManager.FindByEmailAsync(request.email) is not { } User)
            return Result.Success();

        if (User.EmailConfirmed)
            return Result.Faluire(UserErrors.ActiveConfirmedEmail);

        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await _UserManager.GenerateEmailConfirmationTokenAsync(User)));

        _Logger.LogInformation("Confirmation Code: {code}", code);

         SendEmailConfirmation(User, code);

        return Result.Success();
    }

    public async Task<Result> SendResetPassCodeAsync(ForgetPasswordRequest request, CancellationToken cancellationToken)
    {

        if (await _UserManager.FindByEmailAsync(request.email) is not { } user)
            return Result.Success();

        if (!user.EmailConfirmed)
            return Result.Faluire(UserErrors.EmailNotConfirmed);

        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await _UserManager.GeneratePasswordResetTokenAsync(user)));

        _Logger.LogInformation(code);

        var origin = _HttpContextAccessor.HttpContext!.Request.Headers.Origin;

        var HtmlBody = EmailBodyBuilder.GenerateEmailBody("ResetPassword", new Dictionary<string, string>()
       {
           {"{{CODE}}",code },
           {"{{FullName}}" , user.FirstName + " " + user.SecondName},
           { "{{URI}}", $"{origin}/Auth/reset-pss/?email={user.Email}&&code={code}"}
       });

      BackgroundJob.Enqueue(()=>  _EmailSender.SendEmailAsync(user.Email!, "✅ AiGenda Team: Reset Password", HtmlBody));

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {

        if (await _UserManager.FindByEmailAsync(request.email) is not { } user)
            return Result.Success();

        if (!user.EmailConfirmed)
            return Result.Faluire(UserErrors.InvalidCode);

        IdentityResult result = null!;
        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.code));

            result = await _UserManager.ResetPasswordAsync(user, code, request.newpassword);
        }
        catch (FormatException)
        {
            result = IdentityResult.Failed(_UserManager.ErrorDescriber.InvalidToken());
        }

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.FirstOrDefault();

        return Result.Faluire(new Error(error!.Code, error.Description, StatusCodes.Status401Unauthorized));
    }




    private string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    private void SendEmailConfirmation(ExtendedUser User, string code)
    {
        var origin = _HttpContextAccessor.HttpContext?.Request.Headers.Origin;

        var BuilderMessage = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation",

        new Dictionary<string, string>
            {
                        {"{{name}}", User.FirstName + " "+ User.SecondName },
                        {"{{code}}", code },
                        { "{{action_url}}", $"{origin}/Auth/confirm-email?userid={User.Id}&&code={code}" }
            }
        );

    
        BackgroundJob.
            Enqueue(() => _EmailSender.SendEmailAsync(User.Email!, "️✅ AiGenda Team: Email Confirmation", BuilderMessage));
    }


}
