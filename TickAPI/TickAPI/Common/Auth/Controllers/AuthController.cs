using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Enums;

namespace TickAPI.Common.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IJwtService _jwtService;

    public AuthController(IGoogleAuthService googleAuthService, IJwtService jwtService)
    {
        _googleAuthService = googleAuthService;
        _jwtService = jwtService;
    }
    
    // TODO: this is a placeholder method that shows off the general structure of how logging in through Google works
    // in the application. It should be replaced with appropriate login/register endpoints.
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var userDataResult = await _googleAuthService.GetUserDataFromToken(request.IdToken);
        
        if(userDataResult.IsError)
            return StatusCode(userDataResult.StatusCode, userDataResult.ErrorMsg);

        var jwtTokenResult = _jwtService.GenerateJwtToken(userDataResult.Value?.Email, UserRole.Customer);
        
        if(jwtTokenResult.IsError)
            return StatusCode(jwtTokenResult.StatusCode, jwtTokenResult.ErrorMsg);
        
        return Ok(new { token = jwtTokenResult.Value });
    }
    
    public class GoogleLoginRequest
    {
        public string IdToken { get; set; }
    }
}