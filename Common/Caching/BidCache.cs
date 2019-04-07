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
        AsyncClient _client;
        ISerializationContext _serializationContext;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="log"></param>
        /// <param name="serializationContext"></param>
        public BidCache(ILogger<BidCache> log, ISerializationContext serializationContext)
        {
            _log = log;
            var policy = new AsyncClientPolicy
            {
                asyncMaxCommands = 2048,
            };

            _client = new AsyncClient(policy, "aspk-cache.lucent.svc", 3000);
            _serializationContext = serializationContext;
        }

        /// <inheritdoc/>
        public async Task<BidResponse> getEntryAsync(string id)
        {
            using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "bids", "get"))
                try
                {
                    var res = await _client.Get(new Policy { consistencyLevel = ConsistencyLevel.CONSISTENCY_ONE }, default(CancellationToken), new Key("lucent", "bids", id));
                    if (res != null)
                        return await _serializationContext.ReadFrom<BidResponse>(new MemoryStream((byte[])res.GetValue("entry")), false, SerializationFormat.PROTOBUF);
                }
                catch (Exception e)
                {
                    StorageCounters.ErrorCounter.WithLabels("aerospike", "bids", e.GetType().Name).Inc();
                    _log.LogError(e, "Error during get");
                }

            return null;
        }

        /// <inheritdoc/>
        public async Task saveEntries(BidResponse response)
        {
            using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "bids", "save"))
                try
                {
                    var bin = new Bin("entry", await _serializationContext.AsBytes(response, SerializationFormat.PROTOBUF));
                    await _client.Add(new WritePolicy { expiration = 300, }, default(CancellationToken), new Key("lucent", "bids", response.Id), bin);
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