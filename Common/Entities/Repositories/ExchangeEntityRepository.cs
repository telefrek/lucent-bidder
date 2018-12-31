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
    public class ExchangeEntityRespositry : CassandraBaseRepository, IStorageRepository<Exchange, Guid>
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
        public ExchangeEntityRespositry(IServiceProvider provider, ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger logger) : base(session, serializationFormat, serializationContext, logger)
        {
            _provider = provider;
            _tableName = "exchanges";
        }

        /// <inheritdoc/>
        protected override async Task Initialize()
        {
            _getAllExchanges = new SimpleStatement("SELECT etag, format, updated, contents, code FROM {0}".FormatWith(_tableName));

            _getExchangeEntity = await PrepareAsync("SELECT etag, format, updated, contents, code FROM {0} WHERE id = ?".FormatWith(_tableName));

            // no code on insert, only update
            _insertExchangeEntity = await PrepareAsync("INSERT INTO {0} (id, etag, format, updated, contents) VALUES (?, ?, ?, ?, ?, ?) IF NOT EXISTS".FormatWith(_tableName));

            // Allow updates with no code
            _updateExchangeEntity = await PrepareAsync("UPDATE {0} SET etag=?, updated=?, contents=?, format=? WHERE id=? IF etag=?".FormatWith(_tableName));

            // second call to update
            _updateExchangeCode = await PrepareAsync("UPDATE {0} SET code=? WHERE id=? IF etag=?".FormatWith(_tableName));

            _deleteExchangeEntity = await PrepareAsync("DELETE FROM {0} WHERE id=? IF etag=?".FormatWith(_tableName));
        }

        /// <inheritdoc/>
        public async Task<Exchange> Get(Guid id)
        {
            try
            {
                var rowSet = await ExecuteAsync(_getExchangeEntity.Bind(id), "get_" + _tableName);

                return (await ReadAsAsync<Exchange, Guid>(rowSet)).FirstOrDefault();
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
                return await ReadAsAsync<Exchange, Guid>(rowSet);
            }
            catch (DriverException queryError)
            {
                _log.LogError(queryError, "Failed to execute query");
            }

            return new List<Exchange>();
        }

        /// <inheritdoc/>
        public async Task<ICollection<Exchange>> GetAny(Guid id)
        {
            try
            {
                var rowSet = await ExecuteAsync(_getExchangeEntity.Bind(id), "getAny_" + _tableName);

                return await ReadAsAsync<Exchange, Guid>(rowSet);
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
                _log.LogInformation("Inserting new exchange {0}", obj.Id);

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

                var rowSet = await ExecuteAsync(_insertExchangeEntity.Bind(obj.Id, obj.ETag, _serializationFormat.ToString(), DateTime.UtcNow, contents), "insert_" + _tableName);

                if (rowSet != null && obj.Code != null)
                {
                    return await ExecuteAsync(_updateExchangeCode.Bind(obj.Code.ToArray(), obj.Id, obj.ETag), "update_code_" + _tableName) != null;
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
                _log.LogInformation("Removing exchange {0}", obj.Id);
                var rowSet = await ExecuteAsync(_deleteExchangeEntity.Bind(obj.Id, obj.ETag), "delete_" + _tableName);

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

                _log.LogInformation("Updating exchange {0}", obj.Id);
                var contents = new byte[0];
                using (var ms = new MemoryStream())
                {
                    await _serializationContext.WriteTo(obj, ms, true, _serializationFormat);
                    ms.Seek(0, SeekOrigin.Begin);
                    contents = ms.ToArray();
                }

                obj.ETag = contents.CalculateETag();

                var rowSet = await ExecuteAsync(_updateExchangeEntity.Bind(obj.ETag, DateTime.UtcNow, contents, _serializationFormat.ToString(), obj.Id, oldEtag), "update_" + _tableName);

                if (rowSet != null && obj.Code != null)
                    return await ExecuteAsync(_updateExchangeCode.Bind(obj.Code.ToArray(), obj.Id, obj.ETag), "update_code_" + _tableName) != null;

                return rowSet != null;
            }
            catch (DriverException queryError)
            {
                _log.LogError(queryError, "Failed to execute query");
            }

            return false;
        }

        /// <inheritdoc/>
        public override async Task CreateTableAsync() =>
            // optimize this to happen once later
            await ExecuteAsync("CREATE TABLE IF NOT EXISTS {0} (id uuid, etag text, format text, updated timestamp, contents blob, code blob, PRIMARY KEY(id);".FormatWith(_tableName), "create_table_" + _tableName);

        /// <inheritdoc/>
        protected override async Task ReadExtraAsync<T, K>(Row row, T instance)
        {
            if (row.GetColumn("code") != null)
            {
                var contents = row.GetValue<byte[]>("code");
                if (contents != null)
                    using (var ms = new MemoryStream(contents))
                    {
                        try
                        {
                            var asm = AssemblyLoadContext.Default.LoadFromStream(ms);
                            if (asm != null)
                            {
                                var exchgType = asm.GetTypes().FirstOrDefault(t => typeof(AdExchange).IsAssignableFrom(t));
                                if (exchgType != null)
                                {
                                    _log.LogInformation("Creating exchange type : {0}", exchgType.FullName);
                                    var exchg = _provider.CreateInstance(exchgType) as AdExchange;
                                    if (exchg != null)
                                    {
                                        exchg.ExchangeId = (Guid)(object)instance.Id; // <-- this is proof something is ugly with this code...
                                        _log.LogInformation("Loaded {0} ({1})", exchg.Name, exchg.ExchangeId);
                                        await exchg.Initialize(_provider);
                                        (instance as Exchange).Instance = exchg;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _log.LogError(e, "Failed to load exchange : {0}", instance.Id);
                        }
                    }
            }

            await Task.CompletedTask;
        }
    }
}