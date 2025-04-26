namespace TickAPI.Common.Redis.Abstractions;

public interface IRedisService
{
    public Task<string?> GetStringAsync(string key);
    public Task SetStringAsync(string key, string value, TimeSpan? expiry = null);
    public Task<bool> DeleteKeyAsync(string key);
    public Task<T?> GetObjectAsync<T>(string key);
    public Task SetObjectAsync<T>(string key, T value, TimeSpan? expiry = null);
}