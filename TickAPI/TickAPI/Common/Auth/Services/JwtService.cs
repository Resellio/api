using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Time.Abstractions;

namespace TickAPI.Common.Auth.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly IDateTimeService _dateTimeService;

    public JwtService(IConfiguration configuration, IDateTimeService dateTimeService)
    {
        _configuration = configuration;
        _dateTimeService = dateTimeService;
    }
    
    public string GenerateJwtToken(string userEmail, UserRole role)
    {
        // TODO: add some sort of userEmail/Role validation after adding new users is implemented + appropriate tests
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Email, userEmail),
            new Claim(ClaimTypes.Role, role.ToString())
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:Jwt:SecurityKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        int.TryParse(_configuration["Authentication:Jwt:ExpirySeconds"], out var addedSeconds);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Authentication:Jwt:Issuer"],
            claims: claims,
            expires: _dateTimeService.GetCurrentDateTime().AddSeconds(addedSeconds),
            signingCredentials: creds
            );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}