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
    public class BasicCassandraRepository<T> : CassandraBaseRepository, IBasicStorageRepository<T>
        where T : IStorageEntity<string>, new()
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
            await ExecuteAsync("CREATE TABLE IF NOT EXISTS {0} (id text PRIMARY KEY, etag text, format text, updated timestamp, contents blob );".FormatWith(_tableName), "create_table_" + _tableName);

        /// <inheritdoc/>
        protected override void Initialize()
        {
            _getAllStatement = new SimpleStatement("SELECT etag, format, updated, contents FROM {0}".FormatWith(_tableName));

            _getStatement = Prepare("SELECT etag, format, updated, contents FROM {0} WHERE id=?".FormatWith(_tableName));
            _insertStatement = Prepare("INSERT INTO {0} (id, etag, format, updated, contents) VALUES (?, ?, ?, ?, ?) IF NOT EXISTS".FormatWith(_tableName));

            // Check etag
            _updateStatement = Prepare("UPDATE {0} SET etag=?, updated=?, contents=?, format=? WHERE id=? IF etag=?".FormatWith(_tableName));

            // Check etag
            _deleteStatement = Prepare("DELETE FROM {0} WHERE id=? IF etag=?".FormatWith(_tableName));
        }

        /// <inheritdoc/>
        public async Task<ICollection<T>> GetAll()
        {
            var res = new List<T>();
            try
            {
                var rowSet = await ExecuteAsync(_getAllStatement, "getAll_" + _tableName);

                return await ReadAsAsync(rowSet, (row) =>
                    {
                        var contents = row.GetValue<byte[]>("contents");
                        var format = Enum.Parse<SerializationFormat>(row.GetValue<string>("format"));

                        using (var ms = new MemoryStream(contents))
                        {
                            using (var reader = _serializationContext.CreateReader(ms, false, _serializationFormat))
                            {
                                if (reader.HasNext())
                                {
                                    var o = reader.ReadAs<T>();
                                    o.ETag = row.GetValue<string>("etag");
                                    o.Updated = row.GetValue<DateTime>("updated");
                                    return o;
                                }
                            }
                        }

                        return default(T);
                    });
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
        public async Task<T> Get(string key)
        {
            try
            {
                var rowSet = await ExecuteAsync(_getStatement.Bind(key), "get_" + _tableName);

                return (await ReadAsAsync(rowSet, (row) =>
                    {
                        var contents = row.GetValue<byte[]>("contents");
                        var format = Enum.Parse<SerializationFormat>(row.GetValue<string>("format"));

                        using (var ms = new MemoryStream(contents))
                        {
                            using (var reader = _serializationContext.CreateReader(ms, false, _serializationFormat))
                            {
                                if (reader.HasNext())
                                {
                                    var o = reader.ReadAs<T>();
                                    o.ETag = row.GetValue<string>("etag");
                                    o.Updated = row.GetValue<DateTime>("updated");
                                    return o;
                                }
                            }
                        }

                        return default(T);
                    })).FirstOrDefault();
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
        public async Task<ICollection<T>> GetAny(string key)
        {
            var target = await Get(key);
            return target != null ? new List<T>() { target } : new List<T>();
        }

        /// <inheritdoc/>
        public async Task<bool> TryInsert(T obj)
        {
            _log.LogInformation("Inserting new item");
            try
            {
                var contents = new byte[0];
                using (var ms = new MemoryStream())
                {
                    using (var writer = _serializationContext.CreateWriter(ms, true, _serializationFormat))
                    {
                        writer.Write(obj);
                        writer.Flush();
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    contents = ms.ToArray();
                }

                obj.ETag = contents.CalculateETag();

                var rowSet = await ExecuteAsync(_insertStatement.Bind(obj.Id, obj.ETag, _serializationFormat.ToString(), DateTime.UtcNow, contents), "insert_" + _tableName);

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
            _log.LogInformation("Deleting obj");
            try
            {
                var rowSet = await ExecuteAsync(_deleteStatement.Bind(obj.Id, obj.ETag), "delete_" + _tableName);

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
                var contents = new byte[0];
                using (var ms = new MemoryStream())
                {
                    using (var writer = _serializationContext.CreateWriter(ms, true, _serializationFormat))
                    {
                        writer.Write(obj);
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    contents = ms.ToArray();
                }

                obj.ETag = contents.CalculateETag();

                var rowSet = await ExecuteAsync(_updateStatement.Bind(obj.ETag, DateTime.UtcNow, contents, _serializationFormat.ToString(), obj.Id, oldEtag), "update_" + _tableName);

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