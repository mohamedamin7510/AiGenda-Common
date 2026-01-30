using AI_genda_API.Services.AuthService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AI_genda_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _AuthService = authService;

    [HttpPost]
    public async Task<IActionResult> LoginAsync([FromBody] LoginReq loginReq, CancellationToken cancellationToken = default)
    {
        var authResponse = await _AuthService.GetTokenAsync(loginReq.Email, loginReq.Password, cancellationToken);

        return authResponse is null ? BadRequest("The Password or email is wrong.") : Ok(authResponse);
    }


    [HttpPut]
    [Route("refresh")]
    public async Task<IActionResult> GetRefreshTokenAsync([FromBody] ReFTokenReq Tokens ,CancellationToken cancellationToken)
    {
       var response = await  _AuthService.GetRefreshTokenAsync(Tokens.RefreshToken, Tokens.Token!,cancellationToken=default);


        return response is null ? BadRequest("InvalidToken!") : Ok(response);
    }

    [HttpPut]
    [Route("revoke-refresh-token")]
    public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody] ReFTokenReq Tokens, CancellationToken cancellationToken)
    {
        var RevokingResult = await _AuthService.RevokeRefreshTokenAsync(Tokens.RefreshToken, Tokens.Token!, cancellationToken = default);

        return RevokingResult ? Ok("Revoked Successefuly!") : BadRequest("Failed Revoking!");
    }






}
