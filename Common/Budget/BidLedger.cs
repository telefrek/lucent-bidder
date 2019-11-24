using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Cassandra;
using Lucent.Common.Bidding;
using Lucent.Common.Entities;

using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// 
    /// </summary>
    public class BidLedger : CassandraRepository, IBidLedger
    {
        static readonly string _tableName = "ledger";

        PreparedStatement _getRangeStatement;
        PreparedStatement _getDetailedRangeStatement;
        PreparedStatement _insertStatement;

        /// <summary>
        /// Injection constructor
        /// </summary>
        /// <param name="session"></param>
        /// <param name="serializationFormat"></param>
        /// <param name="serializationContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public BidLedger(ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger<BidLedger> logger) :
        base(session, serializationFormat, serializationContext, logger)
        {

        }

        /// <inheritdoc/>
        public override async Task CreateTableAsync() => await ExecuteAsync("CREATE TABLE IF NOT EXISTS {0} (id text, ledgerDate timeuuid, format text, updated timestamp, amount double, contents blob, PRIMARY KEY(id, ledgerDate) ) WITH CLUSTERING ORDER BY (ledgerDate DESC) AND default_time_to_live=1209600 AND gc_grace_seconds = 60 AND caching = {{'keys':'ALL', 'rows_per_partition':'NONE'}} AND compaction={{'compaction_window_size': '1', 'compaction_window_unit': 'HOURS', 'class': 'org.apache.cassandra.db.compaction.TimeWindowCompactionStrategy'}};".FormatWith(_tableName), "create_ledger");


        /// <inheritdoc/>
        public override async Task Initialize(IServiceProvider serviceProvider)
        {
            _log.LogInformation("Initializing ledger queries");
            await CreateTableAsync();
            _getRangeStatement = await PrepareAsync("SELECT amount FROM {0} WHERE id=? AND ledgerDate >= ? AND ledgerDate < ?".FormatWith(_tableName));
            _getDetailedRangeStatement = await PrepareAsync("SELECT amount, contents FROM {0} WHERE id=? AND ledgerDate >= ? AND ledgerDate < ?".FormatWith(_tableName));
            _insertStatement = await PrepareAsync("INSERT INTO {0} (id, ledgerDate, format, updated, amount, contents) VALUES (?, ?, ?, ?, ?, ?)".FormatWith(_tableName));
        }

        /// <inheritdoc/>
        public async Task<bool> TryRecordEntry(string ledgerId, BidEntry source, Dictionary<string, object> metadata)
        {
            try
            {
                if (source.IsRevenue)
                {
                    metadata = metadata ?? new Dictionary<string, object>();
                    metadata.Add("revenue", source.Cost);
                }

                var contents = (byte[])null;

                try
                {
                    contents = metadata != null ? Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata, Formatting.None)) : null;
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Failed to serialize metadata");
                }

                var rowSet = await ExecuteAsync(_insertStatement.Bind(ledgerId, TimeUuid.NewId(DateTime.UtcNow), _serializationFormat.ToString(), DateTime.UtcNow, source.Cost, contents), "insert_" + _tableName);

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
        public async Task<ICollection<LedgerSummary>> TryGetSummary(string entityId, DateTime start, DateTime end, int? numSegments, bool? detailed, bool? clickOnly)
        {
            _log.LogInformation("Getting summary for {0} at : {1}->{2}", entityId, start, end);
            var segments = new List<LedgerSummary>();

            if (numSegments == null) numSegments = 1;

            // Calculate the approximate split value
            var split = end.Subtract(start).TotalSeconds / numSegments.Value;

            var begin = start;
            var next = start.AddSeconds(split);

            for (var i = 0; i < numSegments; ++i)
            {
                var summary = new LedgerSummary
                {
                    Start = begin,
                    End = next,
                    Amount = 0,
                    Bids = 0,
                    Metadata = new Dictionary<string, int>(),
                };

                foreach (var row in await ExecuteAsync((detailed ?? false ? _getDetailedRangeStatement : _getRangeStatement).Bind(entityId, TimeUuid.Min(begin), TimeUuid.Max(next)), "get_ledger_range"))
                {
                    summary.Amount += row.GetValue<double>("amount");
                                    summary.Bids++;
                    if (detailed ?? false && row.GetColumn("contents") != null)
                    {
                        try
                        {
                            var data = row.GetValue<byte[]>("contents");
                            if (data != null)
                            {
                                var obj = JsonConvert.DeserializeObject<Dictionary<string, int>>(Encoding.UTF8.GetString(data));

                                if ((clickOnly ?? false) && obj.ContainsKey(BidOperation.Clicked.ToString().ToLower()))
                                {
                                    foreach (var key in obj.Keys)
                                    {
                                        if (!summary.Metadata.ContainsKey(key))
                                            summary.Metadata.Add(key, 0);
                                        summary.Metadata[key] += obj[key];
                                    }
                                }
                                else if (!(clickOnly ?? false))
                                {
                                    foreach (var key in obj.Keys)
                                    {
                                        if (!summary.Metadata.ContainsKey(key))
                                            summary.Metadata.Add(key, 0);
                                        summary.Metadata[key] += obj[key];
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ......very disappointed with this solution....
                        }
                    }
                }

                segments.Add(summary);
                begin = next;
                next = begin.AddSeconds(split);
            }

            return segments;
        }
    }
}