using Google.Apis.Auth;

namespace TickAPI.Common.Auth.Abstractions;

public interface IGoogleDataFetcher
{
    Task<HttpResponseMessage> FetchUserDataAsync(string accessToken);
}