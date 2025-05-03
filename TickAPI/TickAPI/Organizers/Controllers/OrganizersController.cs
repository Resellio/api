using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Common.Results.Generic;
using TickAPI.Organizers.Abstractions;
using TickAPI.Organizers.DTOs.Request;
using TickAPI.Organizers.DTOs.Response;

namespace TickAPI.Organizers.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizersController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IJwtService _jwtService;
    private readonly IOrganizerService _organizerService;
    private readonly IClaimsService _claimsService;

    public OrganizersController(IGoogleAuthService googleAuthService, IJwtService jwtService,
        IOrganizerService organizerService, IClaimsService claimsService)
    {
        _googleAuthService = googleAuthService;
        _jwtService = jwtService;
        _organizerService = organizerService;
        _claimsService = claimsService;
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<GoogleOrganizerLoginResponseDto>> GoogleLogin([FromBody] GoogleOrganizerLoginDto request)
    {
        var userDataResult = await _googleAuthService.GetUserDataFromAccessToken(request.AccessToken);
        if(userDataResult.IsError)
            return StatusCode(userDataResult.StatusCode, userDataResult.ErrorMsg);
        
        var userData = userDataResult.Value!;

        Result<string> jwtTokenResult;
        var existingOrganizerResult = await _organizerService.GetOrganizerByEmailAsync(userData.Email);
        if (existingOrganizerResult.IsError)
        {
            jwtTokenResult = _jwtService.GenerateJwtToken(userData.Email, UserRole.NewOrganizer);
            
            if(jwtTokenResult.IsError)
                return StatusCode(jwtTokenResult.StatusCode, jwtTokenResult.ErrorMsg);
            
            return new ActionResult<GoogleOrganizerLoginResponseDto>(new GoogleOrganizerLoginResponseDto(jwtTokenResult.Value!, true, false));
        }

        var isVerified = existingOrganizerResult.Value!.IsVerified;

        var role = isVerified ? UserRole.Organizer : UserRole.UnverifiedOrganizer;
        
        jwtTokenResult = _jwtService.GenerateJwtToken(userData.Email, role);
        
        if(jwtTokenResult.IsError)
            return StatusCode(jwtTokenResult.StatusCode, jwtTokenResult.ErrorMsg);
        
        return new ActionResult<GoogleOrganizerLoginResponseDto>(new GoogleOrganizerLoginResponseDto(jwtTokenResult.Value!, false, isVerified));
    }

    [AuthorizeWithPolicy(AuthPolicies.NewOrganizerPolicy)]
    [HttpPost]
    public async Task<ActionResult<CreateOrganizerResponseDto>> CreateOrganizer([FromBody] CreateOrganizerDto request)
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return StatusCode(StatusCodes.Status400BadRequest, emailResult.ErrorMsg);
        }
        var email = emailResult.Value!;
        
        var newOrganizerResult = await _organizerService.CreateNewOrganizerAsync(email, request.FirstName, request.LastName, request.DisplayName);
        if(newOrganizerResult.IsError)
            return StatusCode(newOrganizerResult.StatusCode, newOrganizerResult.ErrorMsg);
        
        var jwtTokenResult = _jwtService.GenerateJwtToken(newOrganizerResult.Value!.Email, newOrganizerResult.Value!.IsVerified ? UserRole.Organizer : UserRole.UnverifiedOrganizer);
        if(jwtTokenResult.IsError)
            return StatusCode(jwtTokenResult.StatusCode, jwtTokenResult.ErrorMsg);
        
        return new ActionResult<CreateOrganizerResponseDto>(new CreateOrganizerResponseDto(jwtTokenResult.Value!));
    }
    
    [AuthorizeWithPolicy(AuthPolicies.AdminPolicy)]
    [HttpPost("verify")]
    public async Task<ActionResult> VerifyOrganizer([FromBody] VerifyOrganizerDto request)
    {
        var verifyOrganizerResult = await _organizerService.VerifyOrganizerByEmailAsync(request.Email);
        
        if(verifyOrganizerResult.IsError)
            return StatusCode(verifyOrganizerResult.StatusCode, verifyOrganizerResult.ErrorMsg);
        
        return Ok();
    }

    [AuthorizeWithPolicy(AuthPolicies.CreatedOrganizerPolicy)]
    [HttpGet("about-me")]
    public async Task<ActionResult<AboutMeOrganizerResponseDto>> AboutMe()
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return StatusCode(StatusCodes.Status400BadRequest, emailResult.ErrorMsg);
        }
        var email = emailResult.Value!;
        
        var organizerResult = await _organizerService.GetOrganizerByEmailAsync(email);
        if (organizerResult.IsError)
            return StatusCode(StatusCodes.Status500InternalServerError,
                "cannot find organizer in database for authorized organizer request");

        var organizer = organizerResult.Value!;

        var aboutMeResponse =
            new AboutMeOrganizerResponseDto(organizer.Email, organizer.FirstName, organizer.LastName, organizer.DisplayName, organizer.IsVerified, organizer.CreationDate);
        return new ActionResult<AboutMeOrganizerResponseDto>(aboutMeResponse);
    }
}