using System.Text.Json;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Responses;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Common.Auth.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private IGoogleDataFetcher _googleDataFetcher;

    public GoogleAuthService(IGoogleDataFetcher googleDataFetcher)
    {
        _googleDataFetcher = googleDataFetcher;
    }

    public async Task<Result<GoogleUserData>> GetUserDataFromAccessToken(string accessToken)
    {
        try
        {
            var response = await _googleDataFetcher.FetchUserDataAsync(accessToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<GoogleUserData>.Failure(StatusCodes.Status401Unauthorized, "Invalid Google access token");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GoogleUserData>(jsonResponse, new JsonSerializerOptions());
            
            if (userInfo == null)
            {
                return Result<GoogleUserData>.Failure(StatusCodes.Status500InternalServerError, "Failed to parse Google user info");
            }
            
            return Result<GoogleUserData>.Success(userInfo);
        }
        catch (Exception ex)
        {
            return Result<GoogleUserData>.Failure(StatusCodes.Status500InternalServerError, $"Error fetching user data: {ex.Message}");
        }
    }
}