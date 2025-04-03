using System.Security.Claims;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Common.Claims.Abstractions;

public interface IClaimsService
{
    Result<string> GetEmailFromClaims(IEnumerable<Claim> claims);
}
