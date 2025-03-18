using TickAPI.Common.Auth.Responses;
using TickAPI.Common.Result;

namespace TickAPI.Common.Auth.Abstractions;

public interface IGoogleAuthService
{
    Task<Result<GoogleUserData>> GetUserDataFromToken(string token); 
}