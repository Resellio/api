using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Auth.Responses;
using TickAPI.Common.Results.Generic;
using TickAPI.Customers.Abstractions;
using TickAPI.Customers.Controllers;
using TickAPI.Customers.DTOs.Request;
using TickAPI.Customers.Models;

namespace TickAPI.Tests.Customers.Controllers;

public class CustomerControllerTests
{
    [Fact]
    public async Task GoogleLogin_WhenAuthSuccessAndCustomerExists_ShouldReturnToken()
    {
        // Arrange
        const string email = "existing@test.com";
        const string accessToken = "valid-google-token";
        const string jwtToken = "valid-jwt-token";
        
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        googleAuthServiceMock.Setup(m => m.GetUserDataFromAccessToken(accessToken))
            .ReturnsAsync(Result<GoogleUserData>.Success(new GoogleUserData(email, "First", "Last")));
    
        var customerServiceMock = new Mock<ICustomerService>();
        customerServiceMock.Setup(m => m.GetCustomerByEmailAsync(email))
            .ReturnsAsync(Result<Customer>.Success(new Customer { Email = email }));
    
        var jwtServiceMock = new Mock<IJwtService>();
        jwtServiceMock.Setup(m => m.GenerateJwtToken(email, UserRole.Customer))
            .Returns(Result<string>.Success(jwtToken));
    
        var sut = new CustomerController(
            googleAuthServiceMock.Object, 
            jwtServiceMock.Object, 
            customerServiceMock.Object);
    
        // Act
        var actionResult = await sut.GoogleLogin(new GoogleCustomerLoginDto(accessToken));
    
        // Assert
        Assert.Equal(jwtToken, actionResult.Value?.Token);
    }
    
    [Fact]
    public async Task GoogleLogin_WhenAuthSuccessAndCustomerDoesNotExist_ShouldCreateCustomerAndReturnToken()
    {
        // Arrange
        const string email = "new@test.com";
        const string accessToken = "valid-google-token";
        const string firstName = "First";
        const string lastName = "Last";
        const string jwtToken = "valid-jwt-token";
        
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        googleAuthServiceMock.Setup(m => m.GetUserDataFromAccessToken(accessToken))
            .ReturnsAsync(Result<GoogleUserData>.Success(new GoogleUserData(email, "First", "Last")));
        
        var customerServiceMock = new Mock<ICustomerService>();
        customerServiceMock.Setup(m => m.GetCustomerByEmailAsync(email))
            .ReturnsAsync(Result<Customer>.Failure(StatusCodes.Status404NotFound, $"customer with email '{email}' not found"));
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
            googleAuthServiceMock.Object, 
            jwtServiceMock.Object, 
            customerServiceMock.Object);
        
        // Act
        var result = await sut.GoogleLogin(new GoogleCustomerLoginDto( accessToken ));
        
        // Assert
        Assert.Equal(jwtToken, result.Value?.Token);
    }
    
    [Fact]
    public async Task AboutMe_WithValidEmailClaim_ShouldReturnCustomerDetails()
    {
        // Arrange
        const string email = "test@example.com";
        const string firstName = "John";
        const string lastName = "Doe";
        var creationDate = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        
        var customer = new Customer
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreationDate = creationDate
        };
        
        var customerServiceMock = new Mock<ICustomerService>();
        customerServiceMock.Setup(m => m.GetCustomerByEmailAsync(email))
            .ReturnsAsync(Result<Customer>.Success(customer));
        
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        var jwtServiceMock = new Mock<IJwtService>();
        
        var sut = new CustomerController(
            googleAuthServiceMock.Object,
            jwtServiceMock.Object,
            customerServiceMock.Object);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email)
        };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
        
        // Act
        var result = await sut.AboutMe();
        
        // Assert
        Assert.Equal(email, result.Value?.Email);
        Assert.Equal(firstName, result.Value?.FirstName);
        Assert.Equal(lastName, result.Value?.LastName);
        Assert.Equal(creationDate, result.Value?.CreationDate);
    }

    [Fact]
    public async Task AboutMe_WithMissingEmailClaim_ShouldReturnBadRequest()
    {
        // Arrange
        var customerServiceMock = new Mock<ICustomerService>();
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        var jwtServiceMock = new Mock<IJwtService>();
        
        var sut = new CustomerController(
            googleAuthServiceMock.Object,
            jwtServiceMock.Object,
            customerServiceMock.Object);
        
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
        
        // Act
        var result = await sut.AboutMe();
        
        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal("missing email claim", objectResult.Value);
    }
}