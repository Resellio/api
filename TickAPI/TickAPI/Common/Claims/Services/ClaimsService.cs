using System.Security.Claims;
using TickAPI.Common.Claims.Abstractions;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Common.Claims.Services;

public class ClaimsService : IClaimsService
{
    public Result<string> GetEmailFromClaims(IEnumerable<Claim> claims)
    {
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (email == null)
            return Result<string>.Failure(StatusCodes.Status400BadRequest, "missing email claim");
        return Result<string>.Success(email);
    }
}
