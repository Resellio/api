namespace TickAPI.Common.Redis.Abstractions;

public interface IRedisService
{
    public Task<string?> GetStringAsync(string key);
    public Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null);
    public Task<bool> DeleteKeyAsync(string key);
    public Task<T?> GetObjectAsync<T>(string key);
    public Task<bool> SetObjectAsync<T>(string key, T value, TimeSpan? expiry = null);
    public Task<bool> KeyExistsAsync(string key);
    public Task<bool> KeyExpireAsync(string key, TimeSpan expiry);
    public Task<long> IncrementValueAsync(string key, long value = 1);
    public Task<long> DecrementValueAsync(string key, long value = 1);
    public Task<long?> GetLongValueAsync(string key);
    public Task<bool> SetLongValueAsync(string key, long value, TimeSpan? expiry = null);
}