using Google.Apis.Auth;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Result;

namespace TickAPI.Common.Auth.Services;

public class GoogleAuthService : IAuthService
{
    private readonly IGoogleTokenValidator _googleTokenValidator;

    public GoogleAuthService(IGoogleTokenValidator googleTokenValidator)
    {
        _googleTokenValidator = googleTokenValidator;
    }

    public async Task<Result<string>> LoginAsync(string idToken)
    {
        try
        {
            var payload = await _googleTokenValidator.ValidateAsync(idToken);
            return Result<string>.Success(payload.Email);
        }
        catch (Exception)
        {
            return Result<string>.Failure(StatusCodes.Status401Unauthorized, "Invalid Google ID token");
        }
    }
}