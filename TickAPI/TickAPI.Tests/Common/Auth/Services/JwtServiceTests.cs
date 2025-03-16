using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Auth.Services;

namespace TickAPI.Tests.Common.Auth.Services;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    
    public JwtServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration
            .Setup(m => m["Authentication:Jwt:SecurityKey"])
            .Returns("ExampleSecurityKey-01234567890123456789"); 
        _mockConfiguration
            .Setup(m => m["Authentication:Jwt:Issuer"])
            .Returns("Issuer");
    }
    
    [Fact]
    public void GenerateJwtToken_WhenGivenValidData_ShouldReturnJwtToken()
    {
        JwtService sut = new JwtService(_mockConfiguration.Object);
        
        var tokenString = sut.GenerateJwtToken("example@test.com", UserRole.Customer);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);
        
        Assert.NotNull(jwt);
        Assert.Equal("Issuer", jwt.Issuer);
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "example@test.com");
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Customer");
    }
}