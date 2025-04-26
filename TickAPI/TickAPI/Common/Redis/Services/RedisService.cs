using System.Text.Json;
using StackExchange.Redis;
using TickAPI.Common.Redis.Abstractions;

namespace TickAPI.Common.Redis.Services;

public class RedisService : IRedisService
{
    private readonly IDatabase _database;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public RedisService(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }
    
    public async Task<string?> GetStringAsync(string key)
    {
        return await RetryAsync(async () =>
        {
            var value = await _database.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        });
    }

    public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        return await RetryAsync(async () =>
        {
            return await _database.StringSetAsync(key, value, expiry);
        });
    }

    public async Task<bool> DeleteKeyAsync(string key)
    {
        return await RetryAsync(async () =>
        {
            return await _database.KeyDeleteAsync(key);
        });
    }

    public async Task<T?> GetObjectAsync<T>(string key)
    {
        var json = await GetStringAsync(key);

        if (string.IsNullOrEmpty(json))
        {
            return default;
        }
        
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task<bool> SetObjectAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        
        return await SetStringAsync(key, json, expiry);
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await RetryAsync(async () =>
        {
            return await _database.KeyExistsAsync(key);
        });
    }

    public async Task<bool> KeyExpireAsync(string key, TimeSpan expiry)
    {
        return await RetryAsync(async () =>
        {
            return await _database.KeyExpireAsync(key, expiry);
        });
    }
    
    private static async Task<T> RetryAsync<T>(Func<Task<T>> action, int retryCount = 3, int millisecondsDelay = 100)
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (ex is RedisConnectionException or RedisTimeoutException && attempt < retryCount)
            {
                attempt++;
                await Task.Delay(millisecondsDelay);
            }
        }
    }
}