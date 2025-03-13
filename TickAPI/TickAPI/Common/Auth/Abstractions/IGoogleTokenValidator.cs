using Google.Apis.Auth;
using TickAPI.Common.Result;

namespace TickAPI.Common.Auth.Abstractions;

public interface IGoogleTokenValidator
{
    Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken);
}