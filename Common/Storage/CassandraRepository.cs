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
    internal class CassandraRepository<T> : ILucentRepository<T>
        where T : IStorageEntity, new()
    {
        ISession _session;
        string _tableName;
        IEntitySerializer<T> _serializer;

        Statement _getAllStatement;
        PreparedStatement _getStatement;
        PreparedStatement _insertStatement;
        PreparedStatement _updateStatement;
        PreparedStatement _deleteStatement;
        IServiceProvider _provider;
        ILogger _log;
        SerializationFormat _format;

        // Need to get table name mapping
        public CassandraRepository(ISession session, IServiceProvider provider, SerializationFormat format)
        {
            _session = session;
            _provider = provider;
            _tableName = typeof(T).Name.ToLowerInvariant();
            _log = provider.GetService<ILoggerFactory>().CreateLogger<CassandraRepository<T>>();
            _serializer = provider.GetService<ISerializationRegistry>().GetSerializer<T>();
            _format = format;

            _getAllStatement = new SimpleStatement("SELECT * FROM {0}".FormatWith(_tableName));

            // optimize this to happen once later
            _session.Execute("CREATE TABLE IF NOT EXISTS {0} (id text PRIMARY KEY, etag text, version int, updated timestamp, contents blob );".FormatWith(_tableName));

            _getStatement = _session.Prepare("SELECT * FROM {0} WHERE id=?".FormatWith(_tableName));
            _insertStatement = _session.Prepare("INSERT INTO {0} (id, etag, version, updated, contents) VALUES (?, ?, ?, ?, ?) IF NOT EXISTS".FormatWith(_tableName));

            // Check etag
            _updateStatement = _session.Prepare("UPDATE {0} SET etag=?, updated=?, contents=? WHERE id=? IF EXISTS AND etag=?".FormatWith(_tableName));

            // Check etag
            _deleteStatement = _session.Prepare("DELETE FROM {0} WHERE id=? IF EXISTS".FormatWith(_tableName));
        }

        public async Task<ICollection<T>> Get()
        {
            var res = new List<T>();
            try
            {
                using (var rowSet = await _session.ExecuteAsync(_getAllStatement))
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

                                    using (var ms = new MemoryStream(contents))
                                    {
                                        using (var reader = ms.WrapSerializer(_provider, _format, true).Reader)
                                        {
                                            if (reader.HasNext())
                                            {
                                                var o = _serializer.Read(reader);
                                                o.ETag = row.GetValue<string>("etag");
                                                o.Updated = row.GetValue<DateTime>("updated");
                                                o.Version = row.GetValue<int>("version");
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

                            using (var ms = new MemoryStream(contents))
                            {
                                using (var reader = ms.WrapSerializer(_provider, _format, true).Reader)
                                {
                                    if (reader.HasNext())
                                    {
                                        var o = _serializer.Read(reader);
                                        o.ETag = row.GetValue<string>("etag");
                                        o.Updated = row.GetValue<DateTime>("updated");
                                        o.Version = row.GetValue<int>("version");
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

        public async Task<T> Get(string key)
        {
            try
            {
                using (var rowSet = await _session.ExecuteAsync(_getStatement.Bind(key)))
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

                                    using (var ms = new MemoryStream(contents))
                                    {
                                        using (var reader = ms.WrapSerializer(_provider, _format, true).Reader)
                                        {
                                            if (reader.HasNext())
                                            {
                                                var o = _serializer.Read(reader);
                                                o.ETag = row.GetValue<string>("etag");
                                                o.Updated = row.GetValue<DateTime>("updated");
                                                o.Version = row.GetValue<int>("version");
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

                            using (var ms = new MemoryStream(contents))
                            {
                                using (var reader = ms.WrapSerializer(_provider, _format, true).Reader)
                                {
                                    if (reader.HasNext())
                                    {
                                        var o = _serializer.Read(reader);
                                        o.ETag = row.GetValue<string>("etag");
                                        o.Updated = row.GetValue<DateTime>("updated");
                                        o.Version = row.GetValue<int>("version");
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

        public async Task<bool> TryInsert(T obj)
        {
            _log.LogInformation("Inserting new item");
            try
            {
                var contents = new byte[0];
                using (var ms = new MemoryStream())
                {
                    using (var writer = ms.WrapSerializer(_provider, _format, true).Writer)
                    {
                        _serializer.Write(writer, obj);
                        writer.Flush();
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    contents = ms.ToArray();
                }

                if (string.IsNullOrEmpty(obj.ETag))
                    obj.ETag = contents.CalculateETag();

                var rowSet = await _session.ExecuteAsync(_insertStatement.Bind(obj.Id, obj.ETag, 1, DateTime.UtcNow, contents));

                return rowSet != null;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to Insert {0}", obj.Id);
            }

            return false;
        }

        public async Task<bool> TryRemove(T obj)
        {
            try
            {
                var rowSet = await _session.ExecuteAsync(_deleteStatement.Bind(obj.Id));

                return rowSet != null;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to Remove {0}", obj.Id);
            }

            return false;
        }

        public async Task<bool> TryUpdate(T obj)
        {
            try
            {
                var oldEtag = obj.ETag;
                var contents = new byte[0];
                using (var ms = new MemoryStream())
                {
                    _serializer.Write(ms.WrapSerializer(_provider, _format, true).Writer, obj);
                    ms.Seek(0, SeekOrigin.Begin);
                    contents = ms.ToArray();
                }
                obj.ETag = contents.CalculateETag();

                var rowSet = await _session.ExecuteAsync(_updateStatement.Bind(obj.ETag, DateTime.UtcNow, contents, obj.Id, oldEtag));

                return rowSet != null;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to Update {0}", obj.Id);
            }

            return false;
        }
    }
}