using Google.Apis.Auth;
using TickAPI.Common.Auth.Abstractions;
using TickAPI.Common.Result;

namespace TickAPI.Common.Auth.Services;

public class GoogleDataFetcher : IGoogleDataFetcher
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public GoogleDataFetcher(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HttpResponseMessage> FetchUserDataAsync(string accessToken)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
        var response = await client.GetAsync(_configuration["Authentication:Google:UserInfoEndpoint"]);
        
        return response;
    }
}