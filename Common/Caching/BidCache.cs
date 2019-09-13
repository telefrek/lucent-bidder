using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aerospike.Client;
using Lucent.Common.Budget;

using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        public async Task<Dictionary<string, int>> getEntryAsync(string id)
        {
            var res = (Record)null;
            using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "bids", "get"))
                try
                {
                    res = await Aerospike.INSTANCE.Get(null, default(CancellationToken), new Key("lucent", "bids", id), "entry");
                }
                catch (AerospikeException e)
                {
                    StorageCounters.ErrorCounter.WithLabels("aerospike", "bids", "get", e.Result.ToString()).Inc();
                    _log.LogError(e, "Error during get");
                }

            if (res != null)
                return JsonConvert.DeserializeObject<Dictionary<string, int>>((string)res.GetValue("entry"));

            return null;
        }

        /// <inheritdoc/>
        public async Task saveEntries(Dictionary<string, int> response, string id)
        {
            var contents = JsonConvert.SerializeObject(response);
            using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "bids", "save"))
                try
                {
                    var bin = new Bin("entry", contents);
                    await Aerospike.INSTANCE.Add(new WritePolicy(Aerospike.INSTANCE.writePolicyDefault) { expiration = 600, }, default(CancellationToken), new Key("lucent", "bids", id), bin);
                }
                catch (AerospikeException e)
                {
                    StorageCounters.ErrorCounter.WithLabels("aerospike", "bids", "save", e.Result.ToString()).Inc();
                    _log.LogError(e, "Error during save: {0} ({1})", id, contents.Length);
                }
        }
    }

    /// <summary>
    /// In memory version
    /// </summary>
    public class MemoryBidCache : IBidCache
    {

        static readonly IMemoryCache _memcache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromSeconds(15), SizeLimit = 100 });

        /// <inheritdoc/>
        public Task<Dictionary<string, int>> getEntryAsync(string id) => Task.FromResult((Dictionary<string, int>)_memcache.Get(id));

        /// <inheritdoc/>
        public Task saveEntries(Dictionary<string, int> response, string id)
        {
            _memcache.Set(id, response, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5), Size = 1 });
            return Task.CompletedTask;
        }
    }
}