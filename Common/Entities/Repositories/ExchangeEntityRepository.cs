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
    /// <typeparam name="Exchange"></typeparam>
    /// <typeparam name="Guid"></typeparam>
    public class ExchangeEntityRespositry : CassandraBaseRepository, IStorageRepository<Exchange, Guid>
    {
        string _tableName;

        Statement _getAllExchanges;
        PreparedStatement _getExchangeEntity;
        PreparedStatement _insertExchangeEntity;
        PreparedStatement _updateExchangeEntity;
        PreparedStatement _deleteExchangeEntity;
        IServiceProvider _provider;

        /// <inheritdoc/>
        public ExchangeEntityRespositry(IServiceProvider provider, ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger logger) : base(session, serializationFormat, serializationContext, logger)
        {
            _provider = provider;
            _tableName = "exchanges";
        }

        /// <inheritdoc/>
        public Task<Exchange> Get(Guid id)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<ICollection<Exchange>> GetAll()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<ICollection<Exchange>> GetAny(Guid id)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> TryInsert(Exchange obj)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> TryRemove(Exchange obj)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> TryUpdate(Exchange obj)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        protected override async Task Initialize()
        {
            throw new NotImplementedException();
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
                using (var ms = new MemoryStream(row.GetValue<byte[]>("code")))
                {
                    try
                    {
                        var asm = AssemblyLoadContext.Default.LoadFromStream(ms);
                        if (asm != null)
                        {
                            var exchgType = asm.GetTypes().FirstOrDefault(t => typeof(IAdExchange).IsAssignableFrom(t));
                            if (exchgType != null)
                            {
                                _log.LogInformation("Creating exchange type : {0}", exchgType.FullName);
                                var exchg = _provider.CreateInstance(exchgType) as IAdExchange;
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