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
    public async Task LoginAsync_WhenTokenValidatorReturnsPayload_ShouldReturnEmailFromPayload()
    {
        var googleTokenValidatorMock = new Mock<IGoogleTokenValidator>();
        googleTokenValidatorMock
            .Setup(m => m.ValidateAsync("validToken"))
            .ReturnsAsync(new GoogleJsonWebSignature.Payload { Email = "example@test.com" });
        var sut = new GoogleAuthService(googleTokenValidatorMock.Object);
        
        var result = await sut.LoginAsync("validToken");
        
        Assert.True(result.IsSuccess);
        Assert.Equal("example@test.com", result.Value);
    }

    [Fact]
    public async Task LoginAsync_WhenTokenValidatorThrowsException_ShouldReturnFailure()
    {
        var googleTokenValidatorMock = new Mock<IGoogleTokenValidator>();
        googleTokenValidatorMock
            .Setup(m => m.ValidateAsync("invalidToken"))
            .Throws(new InvalidJwtException("Invalid Google ID token"));
        var sut = new GoogleAuthService(googleTokenValidatorMock.Object);
        
        var result = await sut.LoginAsync("invalidToken");
        
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.Equal("Invalid Google ID token", result.ErrorMsg);
    }
}