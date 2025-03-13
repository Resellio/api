using Google.Apis.Auth;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Result;

namespace TickAPI.Common.Auth.Services;

public class GoogleAuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public GoogleAuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Result<string>> LoginAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_configuration["Authentication:Google:ClientId"]]
                });
            
            return Result<string>.Success(payload.Email);
        }
        catch (Exception)
        {
            return Result<string>.Failure(StatusCodes.Status401Unauthorized, "Invalid Google token");
        }
    }
}