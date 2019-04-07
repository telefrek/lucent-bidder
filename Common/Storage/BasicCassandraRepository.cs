using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using Lucent.Common.Serialization;
using Lucent.Common.Serialization.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Internal Cassandra storage repository
    /// </summary>
    /// <typeparam name="T">The type of object to store in cassandra</typeparam>
    public class BasicCassandraRepository<T> : CassandraRepository, IStorageRepository<T>
        where T : IStorageEntity, new()
    {
        string _tableName;

        Statement _getAllStatement;
        PreparedStatement _getStatement;
        PreparedStatement _insertStatement;
        PreparedStatement _updateStatement;
        PreparedStatement _deleteStatement;

        /// <inheritdoc/>
        public BasicCassandraRepository(ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger logger) : base(session, serializationFormat, serializationContext, logger)
        {
            _tableName = typeof(T).Name.ToLowerInvariant();
        }

        /// <summary>
        /// Create the table asynchronously
        /// </summary>
        /// <returns></returns>
        public override async Task CreateTableAsync() =>
            // optimize this to happen once later
            await ExecuteAsync("CREATE TABLE IF NOT EXISTS {0} (id text PRIMARY KEY, etag text, format text, updated timestamp, contents blob ) WITH caching = {{'keys': 'all', 'rows_per_partition': 'none'}};".FormatWith(_tableName), "create_table_" + _tableName);

        /// <inheritdoc/>
        public override async Task Initialize(IServiceProvider serviceProvider)
        {
            await CreateTableAsync();
            
            _log.LogInformation("Initializing {0}", typeof(T).Name);
            _getAllStatement = new SimpleStatement("SELECT etag, format, updated, contents FROM {0}".FormatWith(_tableName));

            _getStatement = await PrepareAsync("SELECT etag, format, updated, contents FROM {0} WHERE id=?".FormatWith(_tableName));
            _insertStatement = await PrepareAsync("INSERT INTO {0} (id, etag, format, updated, contents) VALUES (?, ?, ?, ?, ?) IF NOT EXISTS".FormatWith(_tableName));

            // Check etag
            _updateStatement = await PrepareAsync("UPDATE {0} SET etag=?, updated=?, contents=?, format=? WHERE id=? IF etag=?".FormatWith(_tableName));

            // Check etag
            _deleteStatement = await PrepareAsync("DELETE FROM {0} WHERE id=? IF etag=?".FormatWith(_tableName));
        }

        /// <inheritdoc/>
        public async Task<ICollection<T>> GetAll()
        {
            var res = new List<T>();
            try
            {
                var rowSet = await ExecuteAsync(_getAllStatement, "getAll_" + _tableName);
                return await ReadAsAsync<T>(rowSet);
            }
            catch (InvalidQueryException queryError)
            {
                _log.LogError(queryError, "Checking for table missing error");

                if ((queryError.Message ?? "").ToLowerInvariant().Contains("columnfamily") ||
                (queryError.Message ?? "").ToLowerInvariant().Contains("table"))
                    // Recreate
                    await CreateTableAsync();
            }

            return res;
        }

        /// <inheritdoc/>
        public async Task<T> Get(StorageKey key)
        {
            _log.LogInformation("Getting {0}", key.RawValue().First());
            try
            {
                var rowSet = await ExecuteAsync(_getStatement.Bind(key.RawValue()), "get_" + _tableName);

                return (await ReadAsAsync<T>(rowSet)).FirstOrDefault();
            }
            catch (InvalidQueryException queryError)
            {
                _log.LogError(queryError, "Checking for table missing error");

                if ((queryError.Message ?? "").ToLowerInvariant().Contains("columnfamily") ||
                (queryError.Message ?? "").ToLowerInvariant().Contains("table"))
                    // Recreate
                    await CreateTableAsync();
            }

            return default(T);
        }

        /// <inheritdoc/>
        public async Task<ICollection<T>> GetAny(StorageKey key)
        {
            var target = await Get(key);
            return target != null ? new List<T>() { target } : new List<T>();
        }

        /// <inheritdoc/>
        public async Task<bool> TryInsert(T obj)
        {
            try
            {
                var contents = await _serializationContext.AsBytes(obj, _serializationFormat);
                obj.ETag = contents.CalculateETag();

                var parameters = new object[] { null, obj.ETag, _serializationFormat.ToString(), DateTime.UtcNow, contents };
                parameters[0] = obj.Key.RawValue()[0];

                var rowSet = await ExecuteAsync(_insertStatement.Bind(parameters), "insert_" + _tableName);

                return rowSet != null;
            }
            catch (InvalidQueryException queryError)
            {
                _log.LogError(queryError, "Checking for table missing error");

                if ((queryError.Message ?? "").ToLowerInvariant().Contains("columnfamily") ||
                (queryError.Message ?? "").ToLowerInvariant().Contains("table"))
                    // Recreate
                    await CreateTableAsync();
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> TryRemove(T obj)
        {
            _log.LogInformation("Deleting {0} ({1})", obj.Key, obj.ETag);
            try
            {
                var parameters = new object[] { null, obj.ETag };
                parameters[0] = obj.Key.RawValue()[0];
                var rowSet = await ExecuteAsync(_deleteStatement.Bind(parameters), "delete_" + _tableName);

                return rowSet != null;
            }
            catch (InvalidQueryException queryError)
            {
                _log.LogError(queryError, "Checking for table missing error");

                if ((queryError.Message ?? "").ToLowerInvariant().Contains("columnfamily") ||
                (queryError.Message ?? "").ToLowerInvariant().Contains("table"))
                    // Recreate
                    await CreateTableAsync();
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> TryUpdate(T obj)
        {
            _log.LogInformation("Updating entry");
            var oldEtag = obj.ETag;
            try
            {
                var contents = await _serializationContext.AsBytes(obj, _serializationFormat);
                obj.ETag = contents.CalculateETag();

                var parameters = new object[] { obj.ETag, DateTime.UtcNow, contents, _serializationFormat.ToString(), null, oldEtag };
                parameters[4] = obj.Key.RawValue()[0];

                var rowSet = await ExecuteAsync(_updateStatement.Bind(parameters), "update_" + _tableName);

                return rowSet != null;
            }
            catch (InvalidQueryException queryError)
            {
                _log.LogError(queryError, "Checking for table missing error");

                if ((queryError.Message ?? "").ToLowerInvariant().Contains("columnfamily") ||
                (queryError.Message ?? "").ToLowerInvariant().Contains("table"))
                    // Recreate
                    await CreateTableAsync();
            }

            return false;
        }
    }
}