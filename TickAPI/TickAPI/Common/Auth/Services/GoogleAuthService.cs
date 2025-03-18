using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Responses;
using TickAPI.Common.Result;

namespace TickAPI.Common.Auth.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IGoogleTokenValidator _googleTokenValidator;

    public GoogleAuthService(IGoogleTokenValidator googleTokenValidator)
    {
        _googleTokenValidator = googleTokenValidator;
    }

    public async Task<Result<GoogleUserData>> GetUserDataFromToken(string idToken)
    {
        try
        {
            var payload = await _googleTokenValidator.ValidateAsync(idToken);
            var userData = new GoogleUserData(payload.Email, payload.GivenName, payload.FamilyName);
            return Result<GoogleUserData>.Success(userData);
        }
        catch (Exception)
        {
            return Result<GoogleUserData>.Failure(StatusCodes.Status401Unauthorized, "Invalid Google ID token");
        }
    }
}