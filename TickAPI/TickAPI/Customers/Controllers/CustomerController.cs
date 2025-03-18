using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;
using TickAPI.Customers.Abstractions;
using TickAPI.Customers.DTOs.Request;
using TickAPI.Customers.DTOs.Response;

namespace TickAPI.Customers.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly ICustomerService _customerService;
    
    public CustomerController(IAuthService authService, IJwtService jwtService, ICustomerService customerService)
    {
        _authService = authService;
        _jwtService = jwtService;
        _customerService = customerService;
    }
    
    [HttpPost("google-login")]
    public async Task<ActionResult<GoogleLoginResponseDto>> GoogleLogin([FromBody] GoogleLoginDto request)
    {
        var loginResult = await _authService.LoginAsync(request.IdToken);
        if(loginResult.IsError)
            return StatusCode(loginResult.StatusCode, loginResult.ErrorMsg);

        UserRole role;
        bool isNewCustomer;
        
        var existingCustomerResult = await _customerService.GetCustomerByEmailAsync(loginResult.Value!);
        if (existingCustomerResult.IsSuccess)
        {
            role = UserRole.Customer;
            isNewCustomer = false;
        }
        else
        {
            role = UserRole.NewCustomer;
            isNewCustomer = true;
        }
        
        var jwtTokenResult = _jwtService.GenerateJwtToken(loginResult.Value, role);
        if (jwtTokenResult.IsError)
            return StatusCode(jwtTokenResult.StatusCode, jwtTokenResult.ErrorMsg);
        
        return new ActionResult<GoogleLoginResponseDto>(new GoogleLoginResponseDto(jwtTokenResult.Value!, isNewCustomer));
    }

    [AuthorizeWithPolicy(AuthPolicies.NewCustomerPolicy)]
    [HttpPost("google-create-new-account")]
    public async Task<ActionResult<GoogleCreateNewAccountResponseDto>> GoogleCreateNewAccount([FromBody] GoogleCreateNewAccountDto request)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (email == null)
            return StatusCode(StatusCodes.Status400BadRequest, "missing email claim");

        var newCustomerResult = await _customerService.CreateNewCustomerAsync(email, request.FirstName, request.LastName);
        if (newCustomerResult.IsError)
            return StatusCode(newCustomerResult.StatusCode, newCustomerResult.ErrorMsg);
        
        var jwtTokenResult = _jwtService.GenerateJwtToken(newCustomerResult.Value!.Email, UserRole.Customer);
        if (jwtTokenResult.IsError)
            return StatusCode(jwtTokenResult.StatusCode, jwtTokenResult.ErrorMsg);
        
        return new ActionResult<GoogleCreateNewAccountResponseDto>(new GoogleCreateNewAccountResponseDto(jwtTokenResult.Value!));
    }
}