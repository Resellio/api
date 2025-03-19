using System.Text.Json;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Auth.Responses;
using TickAPI.Common.Result;

namespace TickAPI.Common.Auth.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private IConfiguration _configuration;
    private IHttpClientFactory _httpClientFactory;

    public GoogleAuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<GoogleUserData>> GetUserDataFromAccessToken(string accessToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = client.GetAsync(_configuration["Authentication:Google:UserInfoEndpoint"]).Result;
            if (!response.IsSuccessStatusCode)
            {
                return Result<GoogleUserData>.Failure(StatusCodes.Status401Unauthorized, "Invalid Google access token");
            }
            
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GoogleUserData>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
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