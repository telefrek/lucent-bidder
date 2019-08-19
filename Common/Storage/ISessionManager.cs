using Cassandra;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Simple object to handle session management
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// Get the current session
        /// </summary>
        /// <returns>The current session</returns>
        ISession GetSession();
    }

    /// <summary>
    /// Basic implementation of the session manager
    /// </summary>
    public sealed class SessionManager : ISessionManager
    {
        ILogger _log;
        CassandraConfiguration _config;
        ICluster _cluster;
        ISession _session;

        /// <summary>
        /// Injection constructor
        /// </summary>
        /// <param name="logger">A logger for the manager</param>
        /// <param name="options">The cassandra options for this environment</param>
        public SessionManager(ILogger<SessionManager> logger, IOptions<CassandraConfiguration> options)
        {
            _log = logger;
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
        /// Gets the ccurrent session
        /// </summary>
        /// <returns>The current session</returns>
        public ISession GetSession() => _session;
    }
}