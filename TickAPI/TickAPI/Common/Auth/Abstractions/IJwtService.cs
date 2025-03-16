using TickAPI.Common.Auth.Enums;

namespace TickAPI.Common.Auth.Abstractions;

public interface IJwtService
{
    public string GenerateJwtToken(string userEmail, UserRole role);
}