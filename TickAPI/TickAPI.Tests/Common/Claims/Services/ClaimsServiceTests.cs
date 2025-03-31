using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Common.Claims.Services;

namespace TickAPI.Tests.Common.Claims.Services;

public class ClaimsServiceTests
{
    private readonly IClaimsService _claimsService;

    public ClaimsServiceTests()
    {
        _claimsService = new ClaimsService();
    }

    [Fact]
    public void GetEmailFromClaims_WhenEmailInClaims_ShouldReturnEmail()
    {
        // Arrange
        var email = "test@gmail.com";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email)
        };
        
        // Act
        var result = _claimsService.GetEmailFromClaims(claims);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(email, result.Value!);
    }
    
    [Fact]
    public void GetEmailFromClaims_WhenEmailNotInClaims_ShouldReturnFailure()
    {
        // Arrange
        var claims = new List<Claim>();
        
        // Act
        var result = _claimsService.GetEmailFromClaims(claims);
        
        // Assert
        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("missing email claim", result.ErrorMsg);
    }
}
