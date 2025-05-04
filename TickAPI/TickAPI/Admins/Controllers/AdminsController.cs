using Microsoft.AspNetCore.Mvc;
using TickAPI.Admins.Abstractions;
using TickAPI.Admins.DTOs.Request;
using TickAPI.Admins.DTOs.Response;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Customers.DTOs.Request;

namespace TickAPI.Admins.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminsController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IJwtService _jwtService;
    private readonly IAdminService _adminService;
    private readonly IClaimsService _claimsService;
    
    public AdminsController(IGoogleAuthService googleAuthService, IJwtService jwtService, IAdminService adminService, IClaimsService claimsService)
    {
        _googleAuthService = googleAuthService;
        _jwtService = jwtService;
        _adminService = adminService;
        _claimsService = claimsService;
    }
    
    [HttpPost("google-login")]
    public async Task<ActionResult<GoogleAdminLoginResponseDto>> GoogleLogin([FromBody] GoogleAdminLoginDto request)
    {
        var userDataResult = await _googleAuthService.GetUserDataFromAccessToken(request.AccessToken);
        if (userDataResult.IsError)
            return userDataResult.ToObjectResult();

        var userData = userDataResult.Value!;
        
        var adminResult = await _adminService.GetAdminByEmailAsync(userData.Email);
        if (adminResult.IsError)
        {
            return adminResult.ToObjectResult();
        }
        
        var jwtTokenResult = _jwtService.GenerateJwtToken(userData.Email, UserRole.Admin);
        if (jwtTokenResult.IsError)
            return jwtTokenResult.ToObjectResult();
        
        return new ActionResult<GoogleAdminLoginResponseDto>(new GoogleAdminLoginResponseDto(jwtTokenResult.Value!));
    }

    [AuthorizeWithPolicy(AuthPolicies.AdminPolicy)]
    [HttpGet("about-me")]
    public async Task<ActionResult<AboutMeAdminResponseDto>> AboutMe()
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return emailResult.ToObjectResult();
        }
        var email = emailResult.Value!;
        
        var adminResult = await _adminService.GetAdminByEmailAsync(email);
        if (adminResult.IsError)
            return StatusCode(StatusCodes.Status500InternalServerError,
                "cannot find user with admin privilages in database for authorized admin request");

        var admin = adminResult.Value!;
        var aboutMeResponse = new AboutMeAdminResponseDto(admin.Email, admin.Login);
        return new ActionResult<AboutMeAdminResponseDto>(aboutMeResponse);
    }
}
