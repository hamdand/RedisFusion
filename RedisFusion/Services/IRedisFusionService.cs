
namespace RedisFusion.Services
{
    public interface IRedisFusionService
    {
        Task<T> GetCachedObjectAsync<T>(string key);

        Task<T> GetOrAddCachedObjectAsync<T>(string key, Func<Task<T>> getItemFunc, int durationInSeconds = 0);

        Task<T> GetOrAddCachedObjectAsync<T>(string key, Func<Task<T>> getItemFunc, TimeSpan duration);

        Task SetCachedItemAsync<T>(string key, T value, int durationInSeconds = 0);

        Task SetCachedItemAsync<T>(string key, T value, TimeSpan duration);

    }
}