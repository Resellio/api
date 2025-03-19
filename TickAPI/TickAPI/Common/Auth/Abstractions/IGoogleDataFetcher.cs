using Google.Apis.Auth;
using TickAPI.Common.Result;

namespace TickAPI.Common.Auth.Abstractions;

public interface IGoogleDataFetcher
{
    Task<HttpResponseMessage> FetchUserDataAsync(string accessToken);
}