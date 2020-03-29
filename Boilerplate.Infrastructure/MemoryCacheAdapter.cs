using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Boilerplate.Infrastructure
{
    public interface IMemoryCacheAdapter
    {
        Task<T> GetOrSetFromCache<T>(string key, Func<Task<T>> noCacheGenerationFunc);
    }
    public class MemoryCacheAdapter : IMemoryCacheAdapter
    {
        private TimeSpan? _slidingExpiration = null;
        private IMemoryCache _cache = null;

        public MemoryCacheAdapter(IMemoryCache cache, int cachingInSeconds = 0)
        {
            _cache = cache;

            if (cachingInSeconds < 0)
                throw new ArgumentException("Invalid value for sliding cache expiration", nameof(cachingInSeconds));

            if (cachingInSeconds > 1) // not special value to disable caching
                _slidingExpiration = TimeSpan.FromSeconds(cachingInSeconds);
        }

        public async Task<T> GetOrSetFromCache<T>(string key, Func<Task<T>> noCacheGenerationFunc)
        {
            T result = default(T);

            if (!_cache.TryGetValue(key, out result))
            {
                // Key not in cache, so get data.
                result = await noCacheGenerationFunc();

                var cacheEntryOptions = new MemoryCacheEntryOptions();

                if (_slidingExpiration == null) // special value to disable caching
                    cacheEntryOptions.SetAbsoluteExpiration(DateTimeOffset.Now); // will effectively not cache, but still run the caching mechanism (good to test)                
                else
                    cacheEntryOptions.SetSlidingExpiration(_slidingExpiration.Value);

                // Save data in cache.
                _cache.Set(key, result, cacheEntryOptions);
            }

            return result;
        }
    }
}
