using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Enums;
using TickAPI.Customers.Abstractions;
using TickAPI.Customers.DTOs.Request;
using TickAPI.Customers.DTOs.Response;

namespace TickAPI.Customers.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IJwtService _jwtService;
    private readonly ICustomerService _customerService;
    
    public CustomerController(IGoogleAuthService googleAuthService, IJwtService jwtService, ICustomerService customerService)
    {
        _googleAuthService = googleAuthService;
        _jwtService = jwtService;
        _customerService = customerService;
    }
    
    [HttpPost("google-login")]
    public async Task<ActionResult<GoogleLoginResponseDto>> GoogleLogin([FromBody] GoogleLoginDto request)
    {
        var userDataResult = await _googleAuthService.GetUserDataFromToken(request.IdToken);
        if(userDataResult.IsError)
            return StatusCode(userDataResult.StatusCode, userDataResult.ErrorMsg);

        var userData = userDataResult.Value!;
        
        var existingCustomerResult = await _customerService.GetCustomerByEmailAsync(userData.Email);
        if (existingCustomerResult.IsError)
        {
            var newCustomerResult = await _customerService.CreateNewCustomerAsync(userData.Email, userData.FirstName, userData.LastName);
            if (newCustomerResult.IsError)
                return StatusCode(newCustomerResult.StatusCode, newCustomerResult.ErrorMsg);
        }
        
        var jwtTokenResult = _jwtService.GenerateJwtToken(userData.Email, UserRole.Customer);
        if (jwtTokenResult.IsError)
            return StatusCode(jwtTokenResult.StatusCode, jwtTokenResult.ErrorMsg);
        
        return new ActionResult<GoogleLoginResponseDto>(new GoogleLoginResponseDto(jwtTokenResult.Value!));
    }
}