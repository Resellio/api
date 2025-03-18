using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Result;
using TickAPI.Customers.Abstractions;
using TickAPI.Customers.Controllers;
using TickAPI.Customers.DTOs.Request;
using TickAPI.Customers.Models;

namespace TickAPI.Tests.Customers.Controllers;

public class CustomerControllerTests
{
    [Fact]
    public async Task GoogleLogin_WhenAuthSuccessAndCustomerExists_ShouldReturnTokenAndNotNewCustomer()
    {
        const string email = "existing@test.com";
        const string idToken = "valid-google-token";
        const string jwtToken = "valid-jwt-token";
        
        var authServiceMock = new Mock<IAuthService>();
        authServiceMock.Setup(m => m.LoginAsync(idToken))
            .ReturnsAsync(Result<string>.Success(email));
    
        var customerServiceMock = new Mock<ICustomerService>();
        customerServiceMock.Setup(m => m.GetCustomerByEmailAsync(email))
            .ReturnsAsync(Result<Customer>.Success(new Customer { Email = email }));
    
        var jwtServiceMock = new Mock<IJwtService>();
        jwtServiceMock.Setup(m => m.GenerateJwtToken(email, UserRole.Customer))
            .Returns(Result<string>.Success(jwtToken));
    
        var sut = new CustomerController(
            authServiceMock.Object, 
            jwtServiceMock.Object, 
            customerServiceMock.Object);
    
        var actionResult = await sut.GoogleLogin(new GoogleLoginDto(idToken));
    
        Assert.Equal(jwtToken, actionResult.Value?.Token);
        Assert.False(actionResult.Value?.IsNewCustomer);
    }
    
    [Fact]
    public async Task GoogleLogin_WhenAuthSuccessAndCustomerDoesNotExist_ShouldReturnTokenAndNewCustomer()
    {
        const string email = "new@test.com";
        const string idToken = "valid-google-token";
        const string jwtToken = "valid-jwt-token";
        
        var authServiceMock = new Mock<IAuthService>();
        authServiceMock.Setup(m => m.LoginAsync(idToken))
            .ReturnsAsync(Result<string>.Success(email));
        
        var customerServiceMock = new Mock<ICustomerService>();
        customerServiceMock.Setup(m => m.GetCustomerByEmailAsync(email))
            .ReturnsAsync(Result<Customer>.Failure(StatusCodes.Status404NotFound, $"customer with email '{email}' not found"));
        
        var jwtServiceMock = new Mock<IJwtService>();
        jwtServiceMock.Setup(m => m.GenerateJwtToken(email, UserRole.NewCustomer))
            .Returns(Result<string>.Success(jwtToken));
        
        var sut = new CustomerController(
            authServiceMock.Object, 
            jwtServiceMock.Object, 
            customerServiceMock.Object);
        
        var result = await sut.GoogleLogin(new GoogleLoginDto(idToken));
        
        Assert.Equal(jwtToken, result.Value?.Token);
        Assert.True(result.Value?.IsNewCustomer);
    }
    
    [Fact]
    public async Task GoogleCreateNewAccount_WhenCreatingAccountIsSuccessful_ShouldReturnToken()
    {
        const string email = "new@test.com";
        const string firstName = "First";
        const string lastName = "Last";
        const string jwtToken = "valid-jwt-token";
        
        var authServiceMock = new Mock<IAuthService>();
        var customerServiceMock = new Mock<ICustomerService>();
        customerServiceMock.Setup(m => m.CreateNewCustomerAsync(email, firstName, lastName))
            .ReturnsAsync(Result<Customer>.Success(new Customer
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName
            }));
        
        var jwtServiceMock = new Mock<IJwtService>();
        jwtServiceMock.Setup(m => m.GenerateJwtToken(email, UserRole.Customer))
            .Returns(Result<string>.Success(jwtToken));
        
        var sut = new CustomerController(
            authServiceMock.Object, 
            jwtServiceMock.Object, 
            customerServiceMock.Object);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email)
        };
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            }
        };
        
        var result = await sut.GoogleCreateNewAccount(
            new GoogleCreateNewAccountDto( firstName, lastName ));
        
        Assert.Equal(jwtToken, result.Value?.Token);
    }
    
    [Fact]
    public async Task GoogleCreateNewAccount_WhenEmailClaimIsMissing_ShouldReturnBadRequest()
    {
        var authServiceMock = new Mock<IAuthService>();
        var jwtServiceMock = new Mock<IJwtService>();
        var customerServiceMock = new Mock<ICustomerService>();
    
        var sut = new CustomerController(
            authServiceMock.Object, 
            jwtServiceMock.Object, 
            customerServiceMock.Object);
    
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()))
            }
        };
    
        var result = await sut.GoogleCreateNewAccount(
            new GoogleCreateNewAccountDto("First","Last"));
    
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal("missing email claim", objectResult.Value);
    }
}