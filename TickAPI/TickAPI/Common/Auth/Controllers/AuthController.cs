using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Enums;

namespace TickAPI.Common.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;

    public AuthController(IAuthService authService, IJwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }
    
    // TODO: this is a placeholder method that shows off the general structure of how logging in through Google works
    // in the application. It should be replaced with appropriate login/register endpoints.
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var loginResult = await _authService.LoginAsync(request.IdToken);
        
        if(loginResult.IsError)
            return StatusCode(loginResult.StatusCode, loginResult.ErrorMsg);

        var jwtTokenResult = _jwtService.GenerateJwtToken(loginResult.Value, UserRole.Customer);
        
        if(jwtTokenResult.IsError)
            return StatusCode(jwtTokenResult.StatusCode, jwtTokenResult.ErrorMsg);
        
        return Ok(new { token = jwtTokenResult.Value });
    }
    
    public class GoogleLoginRequest
    {
        public string IdToken { get; set; }
    }
}