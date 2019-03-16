using System;
using System.Threading;
using System.Threading.Tasks;
using Aerospike.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Lucent.Common.Caching
{
    /// <summary>
    /// Configuration for hte Aerospike Cache
    /// </summary>
    public class AerospikeConfig
    {
        /// <summary>
        /// Gets/Sets the ServiceName
        /// </summary>
        /// <value></value>
        public string ServiceName { get; set; } = "aspk-cache.lucent.svc";

        /// <summary>
        /// Gets/Sets the port
        /// </summary>
        /// <value></value>
        public int Port { get; set; } = 3000;
    }

    /// <summary>
    /// Implementation of the distributed cache using Aerospike
    /// </summary>
    public class AerospikeCache : IDistributedCache, IDisposable
    {
        AerospikeConfig _config;
        AsyncClient _client;
        Policy _readPolicy;
        WritePolicy _writePolicy;

        /// <summary>
        /// Default Dependency Injection construcctor
        /// </summary>
        public AerospikeCache()
        {
            var policy = new AsyncClientPolicy
            {
                asyncMaxCommands = 256,
            };

            _client = new AsyncClient(policy, "aspk-cache.lucent.svc", 3000);
            // swap
            //_client.Operate(new WritePolicy {}, default(CancellationToken), new Key("", "", ""), Operation.Add(1), Operation.get());

            _readPolicy = new Policy
            {
            };

            _writePolicy = new WritePolicy
            {
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public byte[] Get(string key)
        {
            return GetAsync(key).Result;
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            var record = await _client.Get(_readPolicy, token, new Key("lucent", "lucent", key));
            return record == null ? null : record.GetValue("cache") as byte[];
        }

        /// <inheritdoc/>
        public void Refresh(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            RemoveAsync(key).Wait();
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            await _client.Delete(_writePolicy, token, new Key("lucent", "lucent", key));
        }

        /// <inheritdoc/>
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            SetAsync(key, value, options).Wait();
        }

        /// <inheritdoc/>
        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            var ttl = Math.Max(1, options.SlidingExpiration != null ? (int)options.SlidingExpiration.Value.TotalSeconds : 10);
            await _client.Put(new WritePolicy { expiration = ttl, recordExistsAction = RecordExistsAction.REPLACE }, token, new Key("lucent", "lucent", key), new Bin("cache", value));
        }
    }
}