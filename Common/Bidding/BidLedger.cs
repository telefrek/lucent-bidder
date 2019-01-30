using System.Threading.Tasks;
using Cassandra;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// 
    /// </summary>
    public class BidLedger : CassandraBaseRepository, IBidLedger
    {
        /// <summary>
        /// Injection constructor
        /// </summary>
        /// <param name="session"></param>
        /// <param name="serializationFormat"></param>
        /// <param name="serializationContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public BidLedger(ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger logger) : base(session, serializationFormat, serializationContext, logger)
        {
        }

        /// <inheritdoc/>
        public async Task<bool> TryRecordBid(string entityId, Bid bid)
        {
            _log.LogInformation("Recording bid {0} for {1}", bid.Id, entityId);

            var tableName = "ledger_" + entityId.Replace("-", "");

            var row = await ExecuteAsync("CREATE TABLE IF NOT EXISTS {0} (id text, ledgerDate timeuuid, format text, updated timestamp, contents blob, PRIMARY KEY(id, ledgerDate) ) WITH CLUSTERING ORDER BY (ledgerDate DESC) AND {'compaction_window_size': '7', 'compaction_window_unit': 'DAYS', 'class': 'org.apache.cassandra.db.compaction.TimeWindowCompactionStrategy'};".FormatWith(tableName), "create_bid_ledger");

            row = await ExecuteAsync("INSERT INTO {0} (id, ledgerDate, format, updated, bid) VALUES (?, ?, ?, ?, ?) IF NOT EXISTS".FormatWith(tableName), "insert_ledger_bid");

            return true;
        }

        /// <inheritdoc/>
        protected override Task Initialize() => Task.CompletedTask;
    }
}