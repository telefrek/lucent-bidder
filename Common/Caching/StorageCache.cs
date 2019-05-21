using System;
using System.IO;
using System.Threading.Tasks;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.Extensions.Caching.Memory;

namespace Lucent.Common.Caching
{
    /// <summary>
    /// Multitier cache for fetching data local (memory) or remote
    /// </summary>
    public class StorageCache
    {
        private readonly IMemoryCache _memCache;
        private readonly IStorageManager _storageManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageManager"></param>
        public StorageCache(IStorageManager storageManager)
        {
            _memCache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromSeconds(15), SizeLimit = 1024 });
            _storageManager = storageManager;
        }

        /// <summary>
        /// Get the contents stored at the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="refresh"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> Get<T>(StorageKey key, bool refresh = false) where T : IStorageEntity, new()
        {
            // Get from the memory cache
            var instance = default(T);
            if (_memCache.TryGetValue(key.ToString(), out instance)) return instance;

            // Get from the implementation
            instance = await _storageManager.GetRepository<T>().Get(key);
            if (instance != null)
            {
                // Update the memory cache
                _memCache.Set(key.ToString(), instance, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(300),
                    Size = 1,
                });
            }

            // Return the instance
            return instance;
        }
    }
}