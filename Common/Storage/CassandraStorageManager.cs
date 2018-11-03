using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cassandra;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

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
            var repo = _registry.GetValueOrDefault(typeof(T), null);
            if (repo == null)
            {
                repo = typeof(CassandraBaseRepository).GetMethods().First(m => m.Name == "CreateAsync" && m.IsStatic).MakeGenericMethod(typeof(BasicCassandraRepository<>).MakeGenericType(typeof(T))).Invoke(null, new object[] { _session, _config.Format, _serializationContext, _log });
            }

            return repo as IStorageRepository<T, K>;
        }

        /// <inheritdoc/>
        public void RegisterRepository<R, T, K>() where R : IStorageRepository<T, K> where T : IStorageEntity<K>, new()
        {
            var repository = CassandraBaseRepository.CreateAsync<R>(_session, _config.Format, _serializationContext, _log);
            _registry.AddOrUpdate(typeof(T), repository, (t, oldRepo) => repository);
        }
    }
}