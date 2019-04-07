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
using System.Reflection;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Default repository required for accessing Cassandra resources
    /// </summary>
    public class CassandraRepository
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
        /// Default constructor
        /// </summary>
        /// <param name="session">The current session</param>
        /// <param name="serializationFormat">The serialization format to use</param>
        /// <param name="serializationContext">The current serialization context</param>
        /// <param name="logger">A logger to use for queries</param>
        public CassandraRepository(ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger logger)
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
        /// Creates a repository of the given type
        /// </summary>
        /// <param name="session">The current session</param>
        /// <param name="serializationFormat">The serialization format to use</param>
        /// <param name="serializationContext">The current serialization context</param>
        /// <param name="logger">A logger to use for queries</param>
        /// <typeparam name="T">The type of base repository to create</typeparam>
        /// <returns>An initialized repository of the given type</returns>
        public static T CreateRepo<T>(ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger logger)
        => (T)Activator.CreateInstance(typeof(T), session, serializationFormat, serializationContext, logger);

        /// <summary>
        /// Initialize the repository
        /// </summary>
        /// <returns></returns>
        public virtual Task Initialize(IServiceProvider provider) => Task.CompletedTask;

        /// <summary>
        /// Execute the query asynchronously
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="queryName"></param>
        /// <returns></returns>
        protected async Task<AsyncRowSet> ExecuteAsync(string statement, string queryName)
            => await ExecuteAsync(new SimpleStatement(statement), queryName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="queryName"></param>
        /// <returns></returns>
        protected async Task<AsyncRowSet> ExecuteAsync(IStatement statement, string queryName)
        {
            try
            {
                using (var context = StorageCounters.LatencyHistogram.CreateContext("cassandra", _session.Keyspace, queryName))
                {
                    return new AsyncRowSet(await _session.ExecuteAsync(statement), 128);
                }
            }
            catch (InvalidQueryException queryError)
            {
                _log.LogWarning(queryError, "Checking for table missing error ({0})", queryName);

                if ((queryError.Message ?? "").ToLowerInvariant().Contains("columnfamily") ||
                (queryError.Message ?? "").ToLowerInvariant().Contains("table"))
                {
                    // Recreate
                    await CreateTableAsync();
                    return new AsyncRowSet(await _session.ExecuteAsync(statement), 128);
                }
                throw;
            }
            catch (DriverException e)
            {
                _log.LogError(e, "Failed to execute query {0}", queryName);
                StorageCounters.ErrorCounter.WithLabels("cassandra", _session.Keyspace, e.GetType().Name).Inc();
            }

            // No Results
            return new AsyncRowSet(new RowSet(), 128);
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
                using (var context = StorageCounters.LatencyHistogram.CreateContext("cassandra", _session.Keyspace, "prepare"))
                    return await _session.PrepareAsync(statement);
            }
            catch (InvalidQueryException iqe)
            {
                if ((iqe.Message ?? "").ToLowerInvariant().Contains("columnfamily") ||
                (iqe.Message ?? "").ToLowerInvariant().Contains("table"))
                {
                    await CreateTableAsync();
                    return await _session.PrepareAsync(statement);
                }
                throw;
            }
            catch (DriverException e)
            {
                _log.LogError(e, "Failed to prepare statement : {0}", statement);
                StorageCounters.ErrorCounter.WithLabels("cassandra", _session.Keyspace, e.GetType().Name).Inc();
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected virtual async Task ReadExtraAsync<T>(Row row, T instance) where T : IStorageEntity, new() => await Task.CompletedTask;

        /// <summary>
        /// Reads the rowset completely, using the builder function to create entities
        /// </summary>
        /// <param name="rowSet"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected async Task<ICollection<T>> ReadAsAsync<T>(AsyncRowSet rowSet) where T : IStorageEntity, new()
        {
            var instances = new List<T>();

            foreach(var row in rowSet)
            {

                var contents = row.GetValue<byte[]>("contents");
                var format = Enum.Parse<SerializationFormat>(row.GetValue<string>("format"));
                var etag = row.GetValue<string>("etag");

                using (var ms = new MemoryStream(contents))
                {
                    var obj = await _serializationContext.ReadFrom<T>(ms, false, format);
                    await ReadExtraAsync<T>(row, obj);
                    obj.ETag = etag;
                    instances.Add(obj);
                }
            }
            
            return instances;
        }
    }
}