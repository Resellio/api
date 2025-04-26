using StackExchange.Redis;
using TickAPI.Common.Redis.Abstractions;

namespace TickAPI.Common.Redis.Services;

public class RedisService : IRedisService
{
    private readonly IDatabase _database;

    public RedisService(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }
    
    public Task<string?> GetStringAsync(string key)
    {
        throw new NotImplementedException();
    }

    public Task SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteKeyAsync(string key)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetObjectAsync<T>(string key)
    {
        throw new NotImplementedException();
    }

    public Task SetObjectAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        throw new NotImplementedException();
    }

    private async Task<T> RetryAsync<T>(Func<Task<T>> action, int retryCount = 3, int millisecondsDelay = 100)
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                return await action();
            }
            catch (RedisConnectionException) when (attempt < retryCount)
            {
                attempt++;
                await Task.Delay(millisecondsDelay);
            }
        }
    }
}