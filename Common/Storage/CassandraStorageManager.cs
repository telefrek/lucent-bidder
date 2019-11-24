using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cassandra;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Manager for Cassandra storage
    /// </summary>
    public class CassandraStorageManager : IStorageManager
    {
        IServiceProvider _provider;
        ISessionManager _sessionManager;
        ISerializationContext _serializationContext;
        ILogger _log;
        static readonly ConcurrentDictionary<Type, object> _registry = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="serializationContext"></param>
        /// <param name="sessionManager"></param>
        /// <param name="logger"></param>
        public CassandraStorageManager(IServiceProvider provider, ISerializationContext serializationContext, ISessionManager sessionManager, ILogger<CassandraStorageManager> logger)
        {
            _provider = provider;
            _serializationContext = serializationContext;
            _sessionManager = sessionManager;
            _log = logger;
        }

        /// <inheritdoc/>
        public IStorageRepository<T> GetRepository<T>() where T : IStorageEntity, new()
        {
            var repo = _registry.GetValueOrDefault(typeof(T), null);
            if (repo == null)
            {
                _log.LogInformation("Creating repo for {0}", typeof(T).Name);
                CassandraRepository baseRepo = typeof(CassandraRepository).GetMethods().First(m => m.Name == "CreateRepo" && m.IsStatic).MakeGenericMethod(typeof(BasicCassandraRepository<>).MakeGenericType(typeof(T))).Invoke(null, new object[] { _sessionManager.GetSession(), SerializationFormat.JSON, _serializationContext, _log }) as CassandraRepository;
                if (baseRepo != null)
                    baseRepo.Initialize(_provider).Wait();

                _registry.TryAdd(typeof(T), baseRepo);
                return baseRepo as IStorageRepository<T>;
            }

            return repo as IStorageRepository<T>;
        }

        /// <summary>
        /// Getter for bad design... ugh
        /// </summary>
        /// <value></value>
        public ISession Session { get => _sessionManager.GetSession(); }

        /// <inheritdoc/>
        public void RegisterRepository<R, T>() where R : IStorageRepository<T> where T : IStorageEntity, new()
        {
            _log.LogInformation("Registerring {0} with {1}", typeof(T).Name, typeof(R).Name);

            var repository = _provider.CreateInstance<R>(_sessionManager.GetSession(), SerializationFormat.JSON, _serializationContext, _log);
            if (repository == null)
                _log.LogError("No repo created, failure!");
            else
            {
                if (repository is CassandraRepository)
                    (repository as CassandraRepository).Initialize(_provider);

                _registry.AddOrUpdate(typeof(T), repository, (t, oldRepo) => repository);
            }
        }
    }
}