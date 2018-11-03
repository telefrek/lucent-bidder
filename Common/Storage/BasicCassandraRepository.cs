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
        }

        /// <inheritdoc/>
        protected override async Task Initialize()
        {
            _tableName = typeof(T).Name.ToLowerInvariant();

            _getAllStatement = new SimpleStatement("SELECT id, etag, format, updated, contents FROM {0}".FormatWith(_tableName));

            // optimize this to happen once later
            var results = await ExecuteAsync("CREATE TABLE IF NOT EXISTS {0} (id text PRIMARY KEY, etag text, format text, updated timestamp, contents blob );".FormatWith(_tableName), "create_table_" + _tableName);

            _getStatement = await PrepareAsync("SELECT id, etag, format, updated, contents FROM {0} WHERE id=?".FormatWith(_tableName));
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
                using (var rowSet = await ExecuteAsync(_getAllStatement, "getAll_" + _tableName))
                {
                    var numRows = 0;
                    using (var rowEnum = rowSet.GetEnumerator())
                    {
                        while (!rowSet.IsFullyFetched)
                        {
                            if ((numRows = rowSet.GetAvailableWithoutFetching()) > 0)
                            {
                                for (var i = 0; i < numRows && rowEnum.MoveNext(); ++i)
                                {
                                    var row = rowEnum.Current;
                                    var id = row.GetValue<string>("id");

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
                                                res.Add(o);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                                await rowSet.FetchMoreResultsAsync();
                        }

                        while (rowEnum.MoveNext())
                        {
                            var row = rowEnum.Current;
                            var id = row.GetValue<string>("id");

                            var contents = row.GetValue<byte[]>("contents");
                            var tm = Encoding.UTF8.GetString(contents);
                            var format = Enum.Parse<SerializationFormat>(row.GetValue<string>("format"));

                            using (var ms = new MemoryStream(contents))
                            {
                                using (var reader = _serializationContext.CreateReader(ms, false, _serializationFormat))
                                {
                                    if (reader.HasNext())
                                    {
                                        var o = reader.ReadAs<T>();
                                        o.Id = id;
                                        o.ETag = row.GetValue<string>("etag");
                                        o.Updated = row.GetValue<DateTime>("updated");
                                        res.Add(o);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to Get");
            }

            return res;
        }

        /// <inheritdoc/>
        public async Task<T> Get(string key)
        {
            try
            {
                using (var rowSet = await ExecuteAsync(_getStatement.Bind(key), "get_" + _tableName))
                {
                    var numRows = 0;
                    using (var rowEnum = rowSet.GetEnumerator())
                    {
                        while (!rowSet.IsFullyFetched)
                        {
                            if ((numRows = rowSet.GetAvailableWithoutFetching()) > 0)
                            {
                                for (var i = 0; i < numRows && rowEnum.MoveNext(); ++i)
                                {
                                    var row = rowEnum.Current;
                                    var id = row.GetValue<string>("id");

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
                                }
                            }
                            else
                                await rowSet.FetchMoreResultsAsync();
                        }

                        while (rowEnum.MoveNext())
                        {
                            var row = rowEnum.Current;
                            var id = row.GetValue<string>("id");

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
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to Get {0}", key);
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
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to Insert {0}", obj.Id);
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> TryRemove(T obj)
        {
            try
            {
                var rowSet = await ExecuteAsync(_deleteStatement.Bind(obj.Id, obj.ETag), "delete_" + _tableName);

                return rowSet != null;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to Remove {0}", obj.Id);
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> TryUpdate(T obj)
        {
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
            catch (Exception ex)
            {
                obj.ETag = oldEtag;
                _log.LogError(ex, "Failed to Update {0}", obj.Id);
            }

            return false;
        }
    }
}