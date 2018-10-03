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
    internal class CassandraStorageManager : IStorageManager
    {
        ICluster _cluster;
        ISession _session;
        IServiceProvider _provider;
        ISerializationContext _serializationContext;
        CassandraConfiguration _config;
        ILogger _log;
        ConcurrentDictionary<Type, object> _registry;

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

        public ILucentRepository<T> GetRepository<T>() where T : IStorageEntity, new() => new CassandraRepository<T>(_session, _config.Format, _serializationContext, _log);
    }
}