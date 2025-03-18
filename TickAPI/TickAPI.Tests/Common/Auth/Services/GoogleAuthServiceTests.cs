using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Moq;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Services;
using TickAPI.Common.Result;

namespace TickAPI.Tests.Common.Auth.Services;

public class GoogleAuthServiceTests
{
    [Fact]
    public async Task GetUserDataFromToken_WhenTokenValidatorReturnsPayload_ShouldReturnEmailFromPayload()
    {
        var googleTokenValidatorMock = new Mock<IGoogleTokenValidator>();
        googleTokenValidatorMock
            .Setup(m => m.ValidateAsync("validToken"))
            .ReturnsAsync(new GoogleJsonWebSignature.Payload { Email = "example@test.com", GivenName = "First", FamilyName = "Last"});
        var sut = new GoogleAuthService(googleTokenValidatorMock.Object);
        
        var result = await sut.GetUserDataFromToken("validToken");
        
        Assert.True(result.IsSuccess);
        Assert.Equal("example@test.com", result.Value?.Email);
        Assert.Equal("First", result.Value?.FirstName);
        Assert.Equal("Last", result.Value?.LastName);
    }

    [Fact]
    public async Task GetUserDataFromToken_WhenTokenValidatorThrowsException_ShouldReturnFailure()
    {
        var googleTokenValidatorMock = new Mock<IGoogleTokenValidator>();
        googleTokenValidatorMock
            .Setup(m => m.ValidateAsync("invalidToken"))
            .Throws(new InvalidJwtException("Invalid Google ID token"));
        var sut = new GoogleAuthService(googleTokenValidatorMock.Object);
        
        var result = await sut.GetUserDataFromToken("invalidToken");
        
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.Equal("Invalid Google ID token", result.ErrorMsg);
    }
}