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
        ICluster _cluster;
        ISession _session;
        ISerializationContext _serializationContext;
        CassandraConfiguration _config;
        ILogger _log;
        ConcurrentDictionary<Type, object> _registry;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="serializationContext"></param>
        /// <param name="options"></param>
        /// <param name="log"></param>
        public CassandraStorageManager(ISerializationContext serializationContext, IOptions<CassandraConfiguration> options, ILogger<CassandraStorageManager> log)
        {
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

        /// <summary>
        /// Creates a repository for the given storage entity type
        /// </summary>
        /// <typeparam name="T">The type of entity to manage</typeparam>
        /// <returns></returns>
        public IStorageRepostory<T> GetRepository<T>() where T : IStorageEntity, new() => new CassandraRepository<T>(_session, _config.Format, _serializationContext, _log);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IClusteredRepository<T> GetClusterRepository<T>() where T : IClusteredStorageEntity, new() => new CassandraClusterRepository<T>(_session, _config.Format, _serializationContext, _log);
    }
}