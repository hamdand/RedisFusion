using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using RedisFusion.Utilities;
using System.Text.Json;

namespace RedisFusion.Services
{
    public class RedisFusionService : IRedisFusionService
    {
        private readonly IDistributedCache _cache;
        private readonly RedisFusionConfigurations _config;


        public RedisFusionService(IDistributedCache cache, IOptions<RedisFusionConfigurations> config)
        {
            _cache = cache;
            _config = config.Value;

        }

        public async Task<T> GetOrAddCachedObjectAsync<T>(string key, Func<Task<T>> getItemFunc, int durationInSeconds = 0)
        {

            if (!_config.IsEnabled)
            {
                return await getItemFunc();
            }
            var cachedItem = await _cache.GetAsync(key, CancellationToken.None);
            if (cachedItem != null)
            {
                return deserialize<T>(cachedItem);
            }
            else
            {
                var newItem = await getItemFunc();

                await SetCachedItemAsync(key, newItem, durationInSeconds);

                return newItem;
            }
        }

        public async Task<T> GetOrAddCachedObjectAsync<T>(string key, Func<Task<T>> getItemFunc, TimeSpan duration)
        {

            if (!_config.IsEnabled)
            {
                return await getItemFunc();
            }
            var cachedItem = await _cache.GetAsync(key, CancellationToken.None);
            if (cachedItem != null)
            {
                return deserialize<T>(cachedItem);
            }
            else
            {
                var newItem = await getItemFunc();

                await SetCachedItemAsync(key, newItem, duration);

                return newItem;
            }
        }

        public async Task SetCachedItemAsync<T>(string key, T value, TimeSpan duration)
        {
            var serializedValue = JsonSerializer.Serialize(value);

            await _cache.SetStringAsync(key, serializedValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration
            });
        }

        public async Task SetCachedItemAsync<T>(string key, T value, int durationInSeconds = 0)
        {
            var serializedValue = JsonSerializer.Serialize(value);

            await _cache.SetStringAsync(key, serializedValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = durationInSeconds > 0 ? TimeSpan.FromSeconds(durationInSeconds) : TimeSpan.FromMinutes(_config.ExpirationTimeSpanInMinutes)
            });
        }

        public async Task<T> GetCachedObjectAsync<T>(string key)
        {
            if (!_config.IsEnabled)
            {
                return default;
            }

            var cachedItem = await _cache.GetAsync(key, CancellationToken.None);

            if (cachedItem != null)
            {
                return deserialize<T>(cachedItem);
            }

            return default;
        }

        private T deserialize<T>(byte[] serializedItem)
        {
            var result = JsonSerializer.Deserialize<T>(serializedItem);

            return result;
        }
    }
}
