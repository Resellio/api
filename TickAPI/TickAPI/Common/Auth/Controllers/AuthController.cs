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
        var result = await _authService.LoginAsync(request.IdToken);
        
        if(result.IsError)
            return Unauthorized(result.ErrorMsg);

        var jwtToken = _jwtService.GenerateJwtToken(result.Value, UserRole.Customer);
        
        return Ok(new { token = jwtToken });
    }
    
    public class GoogleLoginRequest
    {
        public string IdToken { get; set; }
    }
}