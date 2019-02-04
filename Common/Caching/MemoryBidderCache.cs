using System;
using System.IO;
using System.Threading.Tasks;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Caching
{
    /// <summary>
    /// 
    /// </summary>
    public class MemoryBidderCache : IBudgetCache
    {
        ILogger _log;
        ISerializationContext _serializationContext;
        IMemoryCache _memCache;
        object _budgetSync = new object();

        /// <summary>
        /// Injection constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serializationContext"></param>
        /// <param name="memcache"></param>
        public MemoryBidderCache(ILogger<MemoryBidderCache> logger, ISerializationContext serializationContext, IMemoryCache memcache)
        {
            _log = logger;
            _serializationContext = serializationContext;
            _memCache = memcache;
        }

        /// <inheritdoc/>
        public Task<double> GetBudget(string key)
        {
            var res = _memCache.Get(key);
            return Task.FromResult(res == null ? 0 : (double)res);
        }

        /// <inheritdoc/>
        public Task<bool> TryRemove(string key)
        {
            _memCache.Remove(key);
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<T> TryRetrieve<T>(string key) where T : class, new()
        {
            var bytes = _memCache.Get(key);
            return bytes != null ? await _serializationContext.ReadFrom<T>(new MemoryStream((byte[])bytes), false, SerializationFormat.PROTOBUF) : default(T);
        }

        /// <inheritdoc/>
        public async Task<bool> TryStore<T>(T instance, string key) where T : class, new()
        {
            var raw = await _serializationContext.AsBytes(instance, SerializationFormat.PROTOBUF);
            _memCache.Set(key, raw, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                Size = raw.Length
            });
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> TryUpdate<T>(T instance, string key) where T : class, new()
        {
            var raw = await _serializationContext.AsBytes(instance, SerializationFormat.PROTOBUF);
            _memCache.Set(key, raw, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                Size = raw.Length
            });
            return true;
        }

        /// <inheritdoc/>
        public Task<bool> TryUpdateBudget(string key, double value)
        {
            lock (_budgetSync)
            {
                var current = _memCache.Get(key);
                _memCache.Set(key, (current == null ? value : (double)current + value), new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
                    Size = sizeof(double),
                });
                return Task.FromResult(true);
            }
        }
    }
}