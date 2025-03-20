using TickAPI.Common.Auth.Enums;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Common.Auth.Abstractions;

public interface IJwtService
{
    public Result<string> GenerateJwtToken(string? userEmail, UserRole role);
}