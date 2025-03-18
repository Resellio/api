using Microsoft.AspNetCore.Http;
using Moq;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Auth.Responses;
using TickAPI.Common.Result;
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
        const string idToken = "valid-google-token";
        const string jwtToken = "valid-jwt-token";
        
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        googleAuthServiceMock.Setup(m => m.GetUserDataFromToken(idToken))
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
        var actionResult = await sut.GoogleLogin(new GoogleLoginDto(idToken));
    
        // Assert
        Assert.Equal(jwtToken, actionResult.Value?.Token);
    }
    
    [Fact]
    public async Task GoogleLogin_WhenAuthSuccessAndCustomerDoesNotExist_ShouldCreateCustomerAndReturnToken()
    {
        // Arrange
        const string email = "new@test.com";
        const string idToken = "valid-google-token";
        const string firstName = "First";
        const string lastName = "Last";
        const string jwtToken = "valid-jwt-token";
        
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        googleAuthServiceMock.Setup(m => m.GetUserDataFromToken(idToken))
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
        var result = await sut.GoogleLogin(new GoogleLoginDto( idToken ));
        
        // Assert
        Assert.Equal(jwtToken, result.Value?.Token);
    }
}