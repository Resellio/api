using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Auth.Services;
using TickAPI.Common.Time.Abstractions;

namespace TickAPI.Tests.Common.Auth.Services;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IDateTimeService> _mockDateTimeService;
    
    public JwtServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration
            .Setup(m => m["Authentication:Jwt:SecurityKey"])
            .Returns("ExampleSecurityKey-01234567890123456789"); 
        _mockConfiguration
            .Setup(m => m["Authentication:Jwt:Issuer"])
            .Returns("Issuer");
        _mockConfiguration
            .Setup(m => m["Authentication:Jwt:ExpirySeconds"])
            .Returns("3600");
        
        _mockDateTimeService = new Mock<IDateTimeService>();
        _mockDateTimeService
            .Setup(m => m.GetCurrentDateTime())
            .Returns(new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc));
        
    }
    
    [Fact]
    public void GenerateJwtToken_WhenGivenValidData_ShouldReturnJwtToken()
    {
        JwtService sut = new JwtService(_mockConfiguration.Object, _mockDateTimeService.Object);
        
        var tokenString = sut.GenerateJwtToken("example@test.com", UserRole.Customer);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);
        
        Assert.NotNull(jwt);
        Assert.Equal("Issuer", jwt.Issuer);
        Assert.Equal(new DateTime(1970, 1, 1, 9, 0, 0, DateTimeKind.Utc), jwt.ValidTo);
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "example@test.com");
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Customer");
    }
}