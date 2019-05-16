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
        private readonly ISerializationContext _serializationContext;
        private readonly IStorageManager _storageManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializationContext"></param>
        /// <param name="storageManager"></param>
        public StorageCache(ISerializationContext serializationContext, IStorageManager storageManager)
        {
            _memCache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromSeconds(15), SizeLimit = 32 * 1024L * 1024L });
            _storageManager = storageManager;
            _serializationContext = serializationContext;
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
            var entry = (byte[])null;
            if (_memCache.TryGetValue(key.ToString(), out entry)) return await _serializationContext.ReadFrom<T>(new MemoryStream(entry), false, SerializationFormat.PROTOBUF);

            // Get from the implementation
            var instance = await _storageManager.GetRepository<T>().Get(key);
            if (instance != null)
            {
                // Apply appropriate serialization
                entry = await _serializationContext.AsBytes(instance, SerializationFormat.PROTOBUF);

                // Update the memory cache
                _memCache.Set(key.ToString(), entry, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(300),
                    Size = entry.Length,
                });
            }

            // Return the instance
            return instance;
        }
    }
}