using TickAPI.Common.Result;

namespace TickAPI.Common.Auth.Abstractions;

public interface IAuthService
{
    Task<Result<string>> LoginAsync(string idToken);
}