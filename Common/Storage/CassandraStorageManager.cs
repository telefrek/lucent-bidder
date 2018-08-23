using System;
using Cassandra;
using Microsoft.Extensions.Options;

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
        CassandraConfiguration _config;

        public CassandraStorageManager(IServiceProvider provider, IOptions<CassandraConfiguration> options)
        {
            _provider = provider;
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

        public ILucentRepository<T,K> GetRepository<T,K>() where T : new() => new CassandraRepository<T,K>(_session, _provider);
    }
}