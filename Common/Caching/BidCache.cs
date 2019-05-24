using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aerospike.Client;
using Lucent.Common.Budget;

using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Caching
{
    /// <summary>
    /// Normal implementation
    /// </summary>
    public class BidCache : IBidCache
    {
        ILogger<BidCache> _log;
        ISerializationContext _serializationContext;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="log"></param>
        /// <param name="serializationContext"></param>
        public BidCache(ILogger<BidCache> log, ISerializationContext serializationContext)
        {
            _log = log;
            _serializationContext = serializationContext;
        }

        /// <inheritdoc/>
        public async Task<BidResponse> getEntryAsync(string id)
        {
            var res = (Record)null;
            using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "bids", "get"))
                try
                {
                    res = await Aerospike.INSTANCE.Get(null, default(CancellationToken), new Key("lucent", "bids", id), "entry");
                }
                catch (Exception e)
                {
                    StorageCounters.ErrorCounter.WithLabels("aerospike", "bids", e.GetType().Name).Inc();
                    _log.LogError(e, "Error during get");
                }

            if (res != null)
                return await _serializationContext.ReadFrom<BidResponse>(new MemoryStream((byte[])res.GetValue("entry")), false, SerializationFormat.PROTOBUF);

            return null;
        }

        /// <inheritdoc/>
        public async Task saveEntries(BidResponse response)
        {
            var contents = await _serializationContext.AsBytes(response, SerializationFormat.PROTOBUF);
            using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "bids", "save"))
                try
                {
                    var bin = new Bin("entry", contents);
                    await Aerospike.INSTANCE.Add(new WritePolicy(Aerospike.INSTANCE.writePolicyDefault) { expiration = 300, }, default(CancellationToken), new Key("lucent", "bids", response.Id), bin);
                }
                catch (Exception e)
                {
                    StorageCounters.ErrorCounter.WithLabels("aerospike", "save", e.GetType().Name).Inc();
                    _log.LogError(e, "Error during save");
                }
        }
    }

    /// <summary>
    /// In memory version
    /// </summary>
    public class MemoryBidCache : IBidCache
    {

        static readonly MemoryCache _memcache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromSeconds(15), SizeLimit = 100 });

        /// <inheritdoc/>
        public Task<BidResponse> getEntryAsync(string id) => Task.FromResult((BidResponse)_memcache.Get(id));

        /// <inheritdoc/>
        public Task saveEntries(BidResponse response)
        {
            _memcache.Set(response.Id, response, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5), Size = 1 });
            return Task.CompletedTask;
        }
    }
}