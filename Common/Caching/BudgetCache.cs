using System;
using System.IO;
using System.Threading.Tasks;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Caching
{
    /// <summary>
    /// 
    /// </summary>
    public class BudgetCache : IBudgetCache
    {
        ILogger _log;
        ISerializationContext _serializationContext;
        IDistributedCache _cache;
        object _budgetSync = new object();

        /// <summary>
        /// Injection constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serializationContext"></param>
        /// <param name="cache"></param>
        public BudgetCache(ILogger<BudgetCache> logger, ISerializationContext serializationContext, IDistributedCache cache)
        {
            _log = logger;
            _serializationContext = serializationContext;
            _cache = cache;
        }

        /// <inheritdoc/>
        public async Task<double> GetBudget(string key)
        {
            var res = await _cache.GetAsync(key) ?? BitConverter.GetBytes(BitConverter.DoubleToInt64Bits(0d));
            return BitConverter.Int64BitsToDouble(BitConverter.ToInt64(res, 0));
        }

        /// <inheritdoc/>
        public async Task<bool> TryRemove(string key)
        {
            await _cache.RemoveAsync(key);
            return true;
        }

        /// <inheritdoc/>
        public async Task<T> TryRetrieve<T>(string key) where T : class, new()
        {
            var bytes = await _cache.GetAsync(key);
            return bytes != null ? await _serializationContext.ReadFrom<T>(new MemoryStream((byte[])bytes), false, SerializationFormat.PROTOBUF) : default(T);
        }

        /// <inheritdoc/>
        public async Task<bool> TryStore<T>(T instance, string key, TimeSpan expiration) where T : class, new()
        {
            var raw = await _serializationContext.AsBytes(instance, SerializationFormat.PROTOBUF);
            await _cache.SetAsync(key, raw, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
            });

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> TryUpdate<T>(T instance, string key, TimeSpan expiration) where T : class, new()
        {
            var raw = await _serializationContext.AsBytes(instance, SerializationFormat.PROTOBUF);
            await _cache.SetAsync(key, raw, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> TryUpdateBudget(string key, double value)
        {
            var current = BitConverter.Int64BitsToDouble(BitConverter.ToInt64(await _cache.GetAsync(key) ?? BitConverter.GetBytes(BitConverter.DoubleToInt64Bits(0d))));

            current += value;
            await _cache.SetAsync(key, BitConverter.GetBytes(BitConverter.DoubleToInt64Bits(current)), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
            });

            return true;
        }
    }
}