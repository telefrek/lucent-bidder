using System;
using System.Collections.Generic;
using System.IO;
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
    public class BidLedger : CassandraBaseRepository, IBidLedger
    {
        static readonly string _tableName = "ledger";

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
        public BidLedger(ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger<BidLedger> logger) : base(session, serializationFormat, serializationContext, logger)
        {

        }

        /// <inheritdoc/>
        public override async Task CreateTableAsync() => await ExecuteAsync("CREATE TABLE IF NOT EXISTS {0} (id text, ledgerDate timeuuid, format text, updated timestamp, contents blob, PRIMARY KEY(id, ledgerDate) ) WITH CLUSTERING ORDER BY (ledgerDate DESC) AND compaction={{'compaction_window_size': '7', 'compaction_window_unit': 'DAYS', 'class': 'org.apache.cassandra.db.compaction.TimeWindowCompactionStrategy'}};".FormatWith(_tableName), "create_ledger");


        /// <inheritdoc/>
        public override async Task Initialize(IServiceProvider serviceProvider)
        {
            _log.LogInformation("Initializing ledger queries");
            await CreateTableAsync();
            _getStatement = await PrepareAsync("SELECT id, ledgerDate, format, updated, contents FROM {0} WHERE id=?".FormatWith(_tableName));
            _insertStatement = await PrepareAsync("INSERT INTO {0} (id, ledgerDate, format, updated, contents) VALUES (?, ?, ?, ?, ?)".FormatWith(_tableName));
        }

        /// <inheritdoc/>
        public async Task<ICollection<BidEntry>> GetLedger(string ledgerId)
        {
            try
            {
                var rowSet = await ExecuteAsync(_getStatement.Bind(ledgerId), "get_" + _tableName);
                return await ReadAsAsync(rowSet);
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


        /// <summary>
        /// Reads the rowset completely, using the builder function to create entities
        /// </summary>
        /// <param name="rowSet"></param>
        /// <returns></returns>
        protected async Task<ICollection<BidEntry>> ReadAsAsync(RowSet rowSet)
        {
            var instances = new List<BidEntry>();

            var numRows = 0;
            using (var rowEnum = rowSet.GetEnumerator())
            {
                while (!rowSet.IsFullyFetched)
                {
                    if ((numRows = rowSet.GetAvailableWithoutFetching()) > 0)
                    {
                        for (var i = 0; i < numRows && rowEnum.MoveNext(); ++i)
                        {
                            var contents = rowEnum.Current.GetValue<byte[]>("contents");
                            var format = Enum.Parse<SerializationFormat>(rowEnum.Current.GetValue<string>("format"));

                            using (var ms = new MemoryStream(contents))
                            {
                                var entry = await _serializationContext.ReadFrom<BidEntry>(ms, false, format);
                                instances.Add(entry);
                            }
                        }
                    }
                    else
                        await rowSet.FetchMoreResultsAsync();
                }

                while (rowEnum.MoveNext())
                {
                    for (var i = 0; i < numRows && rowEnum.MoveNext(); ++i)
                    {
                        var contents = rowEnum.Current.GetValue<byte[]>("contents");
                        var format = Enum.Parse<SerializationFormat>(rowEnum.Current.GetValue<string>("format"));

                        using (var ms = new MemoryStream(contents))
                        {
                            var entry = await _serializationContext.ReadFrom<BidEntry>(ms, false, format);
                            instances.Add(entry);
                        }
                    }
                }

                return instances;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> TryRecordEntry(string ledgerId, BidEntry source)
        {
            _log.LogInformation("Recording {0} for {1}", source.Cost, ledgerId);
            try
            {
                var contents = await _serializationContext.AsBytes(source, _serializationFormat);

                var rowSet = await ExecuteAsync(_insertStatement.Bind(ledgerId, TimeUuid.NewId(), _serializationFormat.ToString(), DateTime.UtcNow, contents), "insert_" + _tableName);

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