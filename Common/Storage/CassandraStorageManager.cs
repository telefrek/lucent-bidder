using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cassandra;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Manager for Cassandra storage
    /// </summary>
    public class CassandraStorageManager : IStorageManager
    {
        IServiceProvider _provider;
        ICluster _cluster;
        ISession _session;
        ISerializationContext _serializationContext;
        CassandraConfiguration _config;
        ILogger _log;
        ConcurrentDictionary<Type, object> _registry;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="serializationContext"></param>
        /// <param name="options"></param>
        /// <param name="log"></param>
        public CassandraStorageManager(IServiceProvider provider, ISerializationContext serializationContext, IOptions<CassandraConfiguration> options, ILogger<CassandraStorageManager> log)
        {
            _provider = provider;
            _serializationContext = serializationContext;
            _log = log;
            _registry = new ConcurrentDictionary<Type, object>();
            _config = options.Value;
            _cluster = new CassandraConnectionStringBuilder
            {
                Username = _config.User,
                Password = _config.Credentials,
                Port = 9042,
                ContactPoints = new string[] { _config.Endpoint }
            }.MakeClusterBuilder().Build();
            _session = _cluster.Connect();
            _session.CreateKeyspaceIfNotExists(_config.Keyspace);
            _session.ChangeKeyspace(_config.Keyspace);
        }

        /// <inheritdoc/>
        public IStorageRepository<T, K> GetRepository<T, K>() where T : IStorageEntity<K>, new()
        {
            if (typeof(T).IsAssignableFrom(typeof(IBasicStorageEntity)))
            {
                return _provider.CreateInstance(typeof(CassandraRepository<>).MakeGenericType(typeof(T)), _session, _config.Format, _serializationContext, _log) as IStorageRepository<T, K>;
            }

            return _registry.GetValueOrDefault(typeof(T), null) as IStorageRepository<T, K>;
        }

        /// <inheritdoc/>
        public void RegisterRepository<T, K>(IStorageRepository<T, K> repository) where T : IStorageEntity<K>, new()
        {
            _registry.AddOrUpdate(typeof(T), repository, (t, oldRepo) => repository);
        }
    }
}