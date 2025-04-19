using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Customers.Abstractions;
using TickAPI.Customers.DTOs.Request;
using TickAPI.Customers.DTOs.Response;

namespace TickAPI.Customers.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IJwtService _jwtService;
    private readonly ICustomerService _customerService;
    private readonly IClaimsService _claimsService;
    
    public CustomersController(IGoogleAuthService googleAuthService, IJwtService jwtService, ICustomerService customerService, IClaimsService claimsService)
    {
        _googleAuthService = googleAuthService;
        _jwtService = jwtService;
        _customerService = customerService;
        _claimsService = claimsService;
    }
    
    [HttpPost("google-login")]
    public async Task<ActionResult<GoogleCustomerLoginResponseDto>> GoogleLogin([FromBody] GoogleCustomerLoginDto request)
    {
        var userDataResult = await _googleAuthService.GetUserDataFromAccessToken(request.AccessToken);
        if(userDataResult.IsError)
            return StatusCode(userDataResult.StatusCode, userDataResult.ErrorMsg);

        var userData = userDataResult.Value!;
        
        var existingCustomerResult = await _customerService.GetCustomerByEmailAsync(userData.Email);
        if (existingCustomerResult.IsError)
        {
            var newCustomerResult = await _customerService.CreateNewCustomerAsync(userData.Email, userData.GivenName, userData.FamilyName);
            if (newCustomerResult.IsError)
                return StatusCode(newCustomerResult.StatusCode, newCustomerResult.ErrorMsg);
        }
        
        var jwtTokenResult = _jwtService.GenerateJwtToken(userData.Email, UserRole.Customer);
        if (jwtTokenResult.IsError)
            return StatusCode(jwtTokenResult.StatusCode, jwtTokenResult.ErrorMsg);
        
        return new ActionResult<GoogleCustomerLoginResponseDto>(new GoogleCustomerLoginResponseDto(jwtTokenResult.Value!));
    }

    [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    [HttpGet("about-me")]
    public async Task<ActionResult<AboutMeCustomerResponseDto>> AboutMe()
    {
        var emailResult = _claimsService.GetEmailFromClaims(User.Claims);
        if (emailResult.IsError)
        {
            return StatusCode(emailResult.StatusCode, emailResult.ErrorMsg);
        }
        var email = emailResult.Value!;
        
        var customerResult = await _customerService.GetCustomerByEmailAsync(email);
        if (customerResult.IsError)
            return StatusCode(StatusCodes.Status500InternalServerError,
                "cannot find customer in database for authorized customer request");

        var customer = customerResult.Value!;

        var aboutMeResponse =
            new AboutMeCustomerResponseDto(customer.Email, customer.FirstName, customer.LastName, customer.CreationDate);
        return new ActionResult<AboutMeCustomerResponseDto>(aboutMeResponse);
    }
}