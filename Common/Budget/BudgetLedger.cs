using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cassandra;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// 
    /// </summary>
    public class BudgetLedger : CassandraBaseRepository, IBudgetLedger
    {
        static readonly string _tableName = "ledgers";

        PreparedStatement _getStatement;
        PreparedStatement _insertStatement;

        /// <summary>
        /// Injection constructor
        /// </summary>
        /// <param name="session"></param>
        /// <param name="serializationFormat"></param>
        /// <param name="serializationContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public BudgetLedger(ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger logger) : base(session, serializationFormat, serializationContext, logger)
        {

        }

        /// <inheritdoc/>
        public override async Task CreateTableAsync() => await ExecuteAsync("CREATE TABLE IF NOT EXISTS {0} (id text, ledgerDate timeuuid, etype bigint, amount decimal, format text, updated timestamp, contents blob, PRIMARY KEY(id, ledgerDate) ) WITH CLUSTERING ORDER BY (ledgerDate DESC) AND {'compaction_window_size': '7', 'compaction_window_unit': 'DAYS', 'class': 'org.apache.cassandra.db.compaction.TimeWindowCompactionStrategy'};".FormatWith(_tableName), "create_ledger");


        /// <inheritdoc/>
        public async Task<RowSet> GetLedger(string ledgerId)
        {
            try
            {
                return await ExecuteAsync(_getStatement.Bind(ledgerId), "get_" + _tableName);
            }
            catch (InvalidQueryException queryError)
            {
                _log.LogError(queryError, "Checking for table missing error");

                if ((queryError.Message ?? "").ToLowerInvariant().Contains("columnfamily") ||
                (queryError.Message ?? "").ToLowerInvariant().Contains("table"))
                    // Recreate
                    await CreateTableAsync();
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<bool> TryRecordEntry<T>(string ledgerId, T source, EntityType eType, decimal amount) where T : class, new()
        {
            _log.LogInformation("Recording {0} for {1}", amount, ledgerId);
            try
            {
                var contents = await _serializationContext.AsBytes(source, _serializationFormat);

                var rowSet = await ExecuteAsync(_insertStatement.Bind(ledgerId, TimeUuid.NewId(), (long)eType, amount, _serializationFormat.ToString(), DateTime.UtcNow, contents), "insert_" + _tableName);

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
        protected override async Task Initialize()
        {

            _getStatement = await PrepareAsync("SELECT etype, amount, format, updated, contents FROM {0} WHERE id=?".FormatWith(_tableName));
            _insertStatement = await PrepareAsync("INSERT INTO {0} (id, ledger, etype, amount, format, updated, contents) VALUES (?, ?, ?, ?, ?, ?) IF NOT EXISTS".FormatWith(_tableName));
        }
    }
}