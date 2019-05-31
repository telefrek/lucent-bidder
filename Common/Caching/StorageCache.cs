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
            if (!refresh && _memCache.TryGetValue(key.ToString(), out instance)) return instance;

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

        /// <summary>
        /// Insert the entity in the cache
        /// </summary>
        /// <param name="entity">The entity to insert</param>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <returns>True if successful</returns>
        public async Task<bool> TryInsert<T>(T entity) where T : IStorageEntity, new()
        {
            var success = await _storageManager.GetRepository<T>().TryInsert(entity);
            if (success)
                _memCache.Set(entity.Key.ToString(), entity, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(300),
                    Size = 1,
                });
            return success;
        }

        /// <summary>
        /// Update the entity in the cache
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <returns>True if successful</returns>
        public async Task<bool> TryUpdate<T>(T entity) where T : IStorageEntity, new()
        {
            var success = await _storageManager.GetRepository<T>().TryUpdate(entity);
            if (success)
                _memCache.Set(entity.Key.ToString(), entity, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(300),
                    Size = 1,
                });
            return success;
        }

        /// <summary>
        /// Remove the entity from the cache
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        /// <param name="includeSource">Flag to remove from the source storage</param>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <returns>True if successful</returns>
        public async Task<bool> TryRemove<T>(T entity, bool includeSource = false) where T : IStorageEntity, new()
        {
            var success = includeSource ? await _storageManager.GetRepository<T>().TryRemove(entity) : true;
            if (success)
                _memCache.Remove(entity.Key.ToString());
            return success;
        }
    }
}