using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Result;
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
    
    public Result<string> GenerateJwtToken(string? userEmail, UserRole role)
    {
        // TODO: add some sort of userEmail/Role validation after adding new users is implemented + appropriate tests
        
        var configurationDataResult = ValidateConfigurationData(userEmail);
        if (configurationDataResult.IsError)
            return Result<string>.PropagateError(configurationDataResult);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Email, userEmail),
            new Claim(ClaimTypes.Role, role.ToString())
        };
        
        var key = configurationDataResult.Value.SecurityKey;
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var addedSeconds = configurationDataResult.Value.ExpirySeconds;

        var token = new JwtSecurityToken(
            issuer: configurationDataResult.Value.Issuer,
            claims: claims,
            expires: _dateTimeService.GetCurrentDateTime().AddSeconds(addedSeconds),
            signingCredentials: creds
            );
        
        return Result<string>.Success(new JwtSecurityTokenHandler().WriteToken(token));
    }
    
    private Result<ConfigurationData> ValidateConfigurationData(string? userEmail)
    {
        if (string.IsNullOrWhiteSpace(userEmail))
            return Result<ConfigurationData>.Failure(StatusCodes.Status400BadRequest, "'userEmail' parameter cannot be null or empty");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:Jwt:SecurityKey"]!));
        if (key.KeySize < 256)
            return Result<ConfigurationData>.Failure(StatusCodes.Status500InternalServerError, "'SecurityKey' must be at least 256 bits");

        if (!int.TryParse(_configuration["Authentication:Jwt:ExpirySeconds"], out var expirySeconds) || expirySeconds <= 0)
            return Result<ConfigurationData>.Failure(StatusCodes.Status500InternalServerError, "'ExpirySeconds' must be a positive integer");
        
        var issuer = _configuration["Authentication:Jwt:Issuer"];
        
        return Result<ConfigurationData>.Success(new ConfigurationData
        {
            SecurityKey = key,
            ExpirySeconds = expirySeconds,
            Issuer = issuer,
        });
    }

    private struct ConfigurationData
    {
        public SymmetricSecurityKey SecurityKey;
        public int ExpirySeconds;
        public string Issuer;
    }
}