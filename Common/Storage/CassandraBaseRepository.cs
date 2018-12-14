using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cassandra;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Logging;
using Lucent.Common;
using Prometheus;
using System.Collections.Generic;
using System.IO;

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
        /// Create the table asynchronously
        /// </summary>
        /// <returns></returns>
        public virtual Task CreateTableAsync() => Task.CompletedTask;

        /// <summary>
        /// Creates a repository of the given type asynchronously
        /// </summary>
        /// <param name="session">The current session</param>
        /// <param name="serializationFormat">The serialization format to use</param>
        /// <param name="serializationContext">The current serialization context</param>
        /// <param name="logger">A logger to use for queries</param>
        /// <typeparam name="T">The type of base repository to create</typeparam>
        /// <returns>An initialized repository of the given type</returns>
        public static T CreateAsync<T>(ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger logger)
        {
            var repo = (T)Activator.CreateInstance(typeof(T), session, serializationFormat, serializationContext, logger);
            (repo as CassandraBaseRepository).Initialize();
            return repo;
        }

        /// <summary>
        /// Initialize the repository
        /// </summary>
        /// <returns></returns>
        protected abstract void Initialize();

        /// <summary>
        /// Sync method
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="queryName"></param>
        /// <returns></returns>
        protected RowSet Execute(string statement, string queryName) => ExecuteAsync(statement, queryName).Result;

        /// <summary>
        /// Sync method
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="queryName"></param>
        /// <returns></returns>
        protected RowSet Execute(IStatement statement, string queryName) => ExecuteAsync(statement, queryName).Result;

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
            try
            {
                using (var context = _queryLatency.CreateContext(queryName))
                {
                    return await _session.ExecuteAsync(statement);
                }
            }
            catch (InvalidQueryException)
            {
                await CreateTableAsync();
                using (var context = _queryLatency.CreateContext(queryName))
                {
                    return await _session.ExecuteAsync(statement);
                }
            }
        }

        /// <summary>
        /// Sync method
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        protected PreparedStatement Prepare(string statement) => PrepareAsync(statement).Result;

        /// <summary>
        /// Prepares the statement asynchronously
        /// </summary>
        /// <param name="statement">The statement to prepare</param>
        /// <returns>The prepared version of the statement</returns>
        protected async Task<PreparedStatement> PrepareAsync(string statement)
        {
            try
            {
                return await _session.PrepareAsync(statement);
            }
            catch (InvalidQueryException)
            {
                await CreateTableAsync();
                return await _session.PrepareAsync(statement);
            }
        }

        /// <summary>
        /// Reads the rowset completely, using the builder function to create entities
        /// </summary>
        /// <param name="rowSet"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected async Task<ICollection<T>> ReadAsAsync<T>(RowSet rowSet) where T : new()
        {
            var instances = new List<T>();

            var numRows = 0;
            using (var rowEnum = rowSet.GetEnumerator())
            {
                while (!rowSet.IsFullyFetched)
                {
                    if ((numRows = rowSet.GetAvailableWithoutFetching()) > 0)
                    {
                        for (var i = 0; i < numRows && rowEnum.MoveNext(); ++i)
                        {
                            var contents = rowEnum.Current.GetValue<byte[]>("contents");
                            var format = Enum.Parse<SerializationFormat>(rowEnum.Current.GetValue<string>("format"));

                            using (var ms = new MemoryStream(contents))
                            {
                                instances.Add(await _serializationContext.ReadFrom<T>(ms, false, format));
                            }
                        }
                    }
                    else
                        await rowSet.FetchMoreResultsAsync();
                }

                while (rowEnum.MoveNext())
                {
                    for (var i = 0; i < numRows && rowEnum.MoveNext(); ++i)
                    {
                        var contents = rowEnum.Current.GetValue<byte[]>("contents");
                        var format = Enum.Parse<SerializationFormat>(rowEnum.Current.GetValue<string>("format"));

                        using (var ms = new MemoryStream(contents))
                        {
                            instances.Add(await _serializationContext.ReadFrom<T>(ms, false, format));
                        }
                    }
                }

                return instances;
            }
        }
    }
}