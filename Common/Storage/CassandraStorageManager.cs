using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cassandra;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

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
        public IStorageRepository<T> GetRepository<T>() where T : IStorageEntity, new()
        {
            var repo = _registry.GetValueOrDefault(typeof(T), null);
            if (repo == null)
            {
                _log.LogInformation("Creating repo for {0}", typeof(T).Name);
                CassandraBaseRepository baseRepo = typeof(CassandraBaseRepository).GetMethods().First(m => m.Name == "CreateRepo" && m.IsStatic).MakeGenericMethod(typeof(BasicCassandraRepository<>).MakeGenericType(typeof(T))).Invoke(null, new object[] { _session, _config.Format, _serializationContext, _log }) as CassandraBaseRepository;
                if (baseRepo != null)
                    baseRepo.Initialize(_provider);

                _registry.TryAdd(typeof(T), baseRepo);
                return baseRepo as IStorageRepository<T>;
            }

            return repo as IStorageRepository<T>;
        }

        /// <summary>
        /// Getter for bad design... ugh
        /// </summary>
        /// <value></value>
        public ISession Session { get => _session; }

        /// <inheritdoc/>
        public void RegisterRepository<R, T>() where R : IStorageRepository<T> where T : IStorageEntity, new()
        {
            var repository = CassandraBaseRepository.CreateRepo<R>(_session, _config.Format, _serializationContext, _log);
            if (repository is CassandraBaseRepository)
                (repository as CassandraBaseRepository).Initialize(_provider);

            _registry.AddOrUpdate(typeof(T), repository, (t, oldRepo) => repository);
        }
    }
}