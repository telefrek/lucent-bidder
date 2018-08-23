using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using Lucent.Common.Serialization;
using Lucent.Common.Serialization.Json;
using Newtonsoft.Json.Linq;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Internal Cassandra storage repository
    /// </summary>
    /// <typeparam name="T">The type of object to store in cassandra</typeparam>
    internal class CassandraRepository<T, K> : ILucentRepository<T, K>
        where T : new()
    {
        ISession _session;
        string _tableName;
        PreparedStatement _getStatement;
        PreparedStatement _insertStatement;
        PreparedStatement _updateStatement;
        PreparedStatement _deleteStatement;
        IServiceProvider _provider;

        // Need to get table name mapping
        public CassandraRepository(ISession session, IServiceProvider provider)
        {
            _session = session;
            _provider = provider;
            _tableName = typeof(T).Name.ToLowerInvariant();

            var keyType = "text";
            var kt = typeof(K);
            if (kt.Equals(typeof(Guid)))
                keyType = "uuid";

            _session.Execute("DROP TABLE IF EXISTS {0}".FormatWith(_tableName));

            // optimize this to happen once later
            _session.Execute("CREATE TABLE IF NOT EXISTS {0} (id {1} PRIMARY KEY, contents text, etag timestamp );".FormatWith(_tableName, keyType));

            _getStatement = _session.Prepare("SELECT * FROM {0} WHERE id=?".FormatWith(_tableName));
            _insertStatement = _session.Prepare("INSERT INTO {0} (id, contents, etag) VALUES (?, ?, ?) IF NOT EXISTS".FormatWith(_tableName));

            // Check etag
            _updateStatement = _session.Prepare("UPDATE {0} SET contents=?, etag=? WHERE id=? IF EXISTS".FormatWith(_tableName));

            // Check etag
            _deleteStatement = _session.Prepare("DELETE FROM {0} WHERE id=? IF EXISTS".FormatWith(_tableName));
        }

        public async Task<T> Get(K key)
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
                                    var id = row.GetValue<K>("id");

                                    var contents = row.GetValue<string>("contents");

                                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
                                    {
                                        using (var reader = ms.WrapSerializer(_provider, SerializationFormat.JSON, true).Reader)
                                        {
                                            if (reader.HasNext())
                                                return new EntitySerializer<T>().Read(reader);
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
                            var id = row.GetValue<K>("id");

                            var contents = row.GetValue<string>("contents");

                            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
                            {
                                using (var reader = ms.WrapSerializer(_provider, SerializationFormat.JSON, true).Reader)
                                {
                                    if (reader.HasNext())
                                        return new EntitySerializer<T>().Read(reader);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return default(T);
        }

        public async Task<bool> TryInsert(T obj, Func<T, K> keyMap)
        {
            try
            {
                var contents = string.Empty;
                using (var ms = new MemoryStream())
                {
                    using (var writer = ms.WrapSerializer(_provider, SerializationFormat.JSON, true).Writer)
                    {
                        new EntitySerializer<T>().Write(writer, obj);
                        writer.Flush();
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    contents = Encoding.UTF8.GetString(ms.ToArray());
                }

                var rowSet = await _session.ExecuteAsync(_insertStatement.Bind(keyMap.Invoke(obj), contents, DateTime.UtcNow));

                return rowSet != null;
            }
            catch
            {

            }

            return false;
        }

        public async Task<bool> TryRemove(T obj, Func<T, K> keyMap)
        {
            try
            {
                var rowSet = await _session.ExecuteAsync(_deleteStatement.Bind(keyMap.Invoke(obj)));

                return rowSet != null;
            }
            catch
            {

            }

            return false;
        }

        public async Task<bool> TryUpdate(T obj, Func<T, K> keyMap)
        {
            try
            {
                var contents = string.Empty;
                using (var ms = new MemoryStream())
                {
                    new EntitySerializer<T>().Write(ms.WrapSerializer(_provider, SerializationFormat.JSON, true).Writer, obj);
                    ms.Seek(0, SeekOrigin.Begin);
                    contents = Encoding.UTF8.GetString(ms.ToArray());
                }

                var rowSet = await _session.ExecuteAsync(_updateStatement.Bind(contents, DateTime.UtcNow, keyMap.Invoke(obj)));

                return rowSet != null;
            }
            catch
            {

            }

            return false;
        }
    }
}