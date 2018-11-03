using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cassandra;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Logging;
using Lucent.Common;
using Prometheus;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Default repository required for accessing Cassandra resources
    /// </summary>
    public abstract class CassandraBaseRepository
    {
        /// <summary>
        /// The default logger
        /// </summary>
        protected ILogger _log;

        /// <summary>
        /// The desired serialization format
        /// </summary>
        protected SerializationFormat _serializationFormat;

        /// <summary>
        /// The current serialization context
        /// </summary>
        protected ISerializationContext _serializationContext;

        /// <summary>
        /// The current session
        /// </summary>
        ISession _session;

        /// <summary>
        /// Metric for tracking query latencies
        /// </summary>
        /// <value></value>
        static Histogram _queryLatency = Metrics.CreateHistogram("query_latency", "Latency for Cassandra queries", new HistogramConfiguration
        {
            LabelNames = new string[] { "query" },
            Buckets = new double[] { 0.001, 0.005, 0.010, 0.015, 0.020, 0.025, 0.050, 0.075, 0.100 },
        });

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="session">The current session</param>
        /// <param name="serializationFormat">The serialization format to use</param>
        /// <param name="serializationContext">The current serialization context</param>
        /// <param name="logger">A logger to use for queries</param>
        public CassandraBaseRepository(ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger logger)
        {
            _log = logger;
            _session = session;
            _serializationFormat = serializationFormat;
            _serializationContext = serializationContext;
        }

        /// <summary>
        /// Creates a repository of the given type asynchronously
        /// </summary>
        /// <param name="session">The current session</param>
        /// <param name="serializationFormat">The serialization format to use</param>
        /// <param name="serializationContext">The current serialization context</param>
        /// <param name="logger">A logger to use for queries</param>
        /// <typeparam name="T">The type of base repository to create</typeparam>
        /// <returns>An initialized repository of the given type</returns>
        public static async Task<T> CreateAsync<T>(ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger logger) where T : CassandraBaseRepository, new()
        {
            var repo = Activator.CreateInstance(typeof(T), session, serializationFormat, serializationContext, logger) as T;
            await repo.Initialize();
            return repo;
        }

        /// <summary>
        /// Initialize the repository
        /// </summary>
        /// <returns></returns>
        protected abstract Task Initialize();

        /// <summary>
        /// Execute the query asynchronously
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="queryName"></param>
        /// <returns></returns>
        protected async Task<RowSet> ExecuteAsync(string statement, string queryName)
            => await ExecuteAsync(new SimpleStatement(statement), queryName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="queryName"></param>
        /// <returns></returns>
        protected async Task<RowSet> ExecuteAsync(IStatement statement, string queryName)
        {
            using (var context = _queryLatency.CreateContext(queryName))
            {
                return await _session.ExecuteAsync(statement);
            }
        }

        /// <summary>
        /// Prepares the statement asynchronously
        /// </summary>
        /// <param name="statement">The statement to prepare</param>
        /// <returns>The prepared version of the statement</returns>
        protected async Task<PreparedStatement> PrepareAsync(string statement)
            => await _session.PrepareAsync(statement);
    }
}