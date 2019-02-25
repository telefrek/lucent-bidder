using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Cassandra;
using Lucent.Common.Exchanges;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Entities.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public class ExchangeEntityRespositry : CassandraBaseRepository, IStorageRepository<Exchange>
    {
        string _tableName;

        Statement _getAllExchanges;
        PreparedStatement _getExchangeEntity;
        PreparedStatement _insertExchangeEntity;
        PreparedStatement _updateExchangeEntity;
        PreparedStatement _updateExchangeCode;
        PreparedStatement _deleteExchangeEntity;
        IServiceProvider _provider;

        /// <inheritdoc/>
        public ExchangeEntityRespositry(ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger logger, IServiceProvider provider) : base(session, serializationFormat, serializationContext, logger)
        {
            _tableName = "exchange";
            _provider = provider;
        }

        /// <inheritdoc/>
        public override async Task Initialize(IServiceProvider provider)
        {
            await CreateTableAsync();
            _getAllExchanges = new SimpleStatement("SELECT etag, format, updated, contents, code FROM {0}".FormatWith(_tableName));

            _getExchangeEntity = await PrepareAsync("SELECT etag, format, updated, contents, code FROM {0} WHERE id=?".FormatWith(_tableName));

            // no code on insert, only update
            _insertExchangeEntity = await PrepareAsync("INSERT INTO {0} (id, etag, format, updated, contents) VALUES (?, ?, ?, ?, ?) IF NOT EXISTS".FormatWith(_tableName));

            // Allow updates with no code
            _updateExchangeEntity = await PrepareAsync("UPDATE {0} SET etag=?, updated=?, contents=?, format=? WHERE id=? IF etag=?".FormatWith(_tableName));

            // second call to update
            _updateExchangeCode = await PrepareAsync("UPDATE {0} SET code=? WHERE id=?".FormatWith(_tableName));

            _deleteExchangeEntity = await PrepareAsync("DELETE FROM {0} WHERE id=? IF etag=?".FormatWith(_tableName));
        }

        /// <inheritdoc/>
        public override async Task CreateTableAsync() =>
            // optimize this to happen once later
            await ExecuteAsync("CREATE TABLE IF NOT EXISTS {0} (id uuid, etag text, format text, updated timestamp, contents blob, code blob, PRIMARY KEY(id) );".FormatWith(_tableName), "create_table_" + _tableName);

        /// <inheritdoc/>
        public async Task<Exchange> Get(StorageKey id)
        {
            try
            {
                var rowSet = await ExecuteAsync(_getExchangeEntity.Bind(id.RawValue()), "get_" + _tableName);

                return (await ReadAsAsync<Exchange>(rowSet)).FirstOrDefault();
            }
            catch (DriverException queryError)
            {
                _log.LogError(queryError, "Failed to execute query");
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<ICollection<Exchange>> GetAll()
        {
            try
            {
                var rowSet = await ExecuteAsync(_getAllExchanges, "getAll_" + _tableName);
                return await ReadAsAsync<Exchange>(rowSet);
            }
            catch (DriverException queryError)
            {
                _log.LogError(queryError, "Failed to execute query");
            }

            return new List<Exchange>();
        }

        /// <inheritdoc/>
        public async Task<ICollection<Exchange>> GetAny(StorageKey id)
        {
            try
            {
                var rowSet = await ExecuteAsync(_getExchangeEntity.Bind(id.RawValue()), "getAny_" + _tableName);

                return await ReadAsAsync<Exchange>(rowSet);
            }
            catch (DriverException queryError)
            {
                _log.LogError(queryError, "Failed to execute query");
            }

            return new List<Exchange>();
        }

        /// <inheritdoc/>
        public async Task<bool> TryInsert(Exchange obj)
        {
            try
            {
                _log.LogInformation("Inserting new exchange {0}", obj.Key);

                if (obj.Code != null && obj.LastCodeUpdate.IsNullOrDefault())
                    obj.LastCodeUpdate = DateTime.UtcNow;

                var contents = new byte[0];
                using (var ms = new MemoryStream())
                {
                    await _serializationContext.WriteTo(obj, ms, true, _serializationFormat);
                    ms.Seek(0, SeekOrigin.Begin);
                    contents = ms.ToArray();
                }

                obj.ETag = contents.CalculateETag();

                var parameters = new object[] { null, obj.ETag, _serializationFormat.ToString(), DateTime.UtcNow, contents };
                parameters[0] = obj.Key.RawValue()[0];

                var rowSet = await ExecuteAsync(_insertExchangeEntity.Bind(parameters), "insert_" + _tableName);

                if (rowSet != null && obj.Code != null)
                {
                    parameters = new object[] { obj.Code.ToArray(), null };
                    _log.LogInformation("Updating exchange code for {0} ({1} bytes)", obj.Key, ((byte[])parameters[0]).Length);
                    parameters[1] = obj.Key.RawValue()[0];
                    return await ExecuteAsync(_updateExchangeCode.Bind(parameters), "update_code_" + _tableName) != null;
                }

                return rowSet != null;
            }
            catch (DriverException queryError)
            {
                _log.LogError(queryError, "Failed to execute query");
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> TryRemove(Exchange obj)
        {
            try
            {
                _log.LogInformation("Removing exchange {0}", obj.Key);
                var parameters = new object[] { null, obj.ETag };
                parameters[0] = obj.Key.RawValue()[0];
                var rowSet = await ExecuteAsync(_deleteExchangeEntity.Bind(parameters), "delete_" + _tableName);

                return rowSet != null;
            }
            catch (DriverException queryError)
            {
                _log.LogError(queryError, "Failed to execute query");
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> TryUpdate(Exchange obj)
        {
            var oldEtag = obj.ETag;
            try
            {
                if (obj.Code != null && obj.LastCodeUpdate.IsNullOrDefault())
                    obj.LastCodeUpdate = DateTime.UtcNow;

                _log.LogInformation("Updating exchange {0}", obj.Key);
                var contents = new byte[0];
                using (var ms = new MemoryStream())
                {
                    await _serializationContext.WriteTo(obj, ms, true, _serializationFormat);
                    ms.Seek(0, SeekOrigin.Begin);
                    contents = ms.ToArray();
                }

                obj.ETag = contents.CalculateETag();

                var parameters = new object[] { obj.ETag, DateTime.UtcNow, contents, _serializationFormat.ToString(), null, oldEtag };
                parameters[4] = obj.Key.RawValue()[0];

                var rowSet = await ExecuteAsync(_updateExchangeEntity.Bind(parameters), "update_" + _tableName);

                if (rowSet != null && obj.Code != null)
                {
                    parameters = new object[] { obj.Code.ToArray(), null };
                    _log.LogInformation("Updating exchange code for {0} ({1} bytes)", obj.Key, ((byte[])parameters[0]).Length);
                    parameters[1] = obj.Key.RawValue()[0];
                    return await ExecuteAsync(_updateExchangeCode.Bind(parameters), "update_code_" + _tableName) != null;
                }

                return rowSet != null;
            }
            catch (DriverException queryError)
            {
                _log.LogError(queryError, "Failed to execute query");
            }

            return false;
        }

        /// <inheritdoc/>
        protected override async Task ReadExtraAsync<T>(Row row, T instance)
        {
            try
            {
                if (row.GetColumn("code") != null)
                {
                    _log.LogInformation("Loading code for exchange {0}", instance.Key.ToString());
                    var contents = row.GetValue<byte[]>("code");
                    if (contents != null)
                        (instance as Exchange).Code = new MemoryStream(contents);
                }
                else
                    _log.LogInformation("No code for exchange {0}", instance.Key.ToString());
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to read the code segment");
            }

            await Task.CompletedTask;
        }
    }
}