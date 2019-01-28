using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Entities.Repositories
{
    /// <summary>
    /// Manages ledger entries
    /// </summary>
    public class LedgerEntryRepository : CassandraBaseRepository, IStorageRepository<LedgerEntry>
    {
        string _tableName;

        Statement _getAllLedgers;
        PreparedStatement _getLedgerEntry;
        PreparedStatement _getFullLedger;
        PreparedStatement _insertLedgerEntry;
        PreparedStatement _updateLedgerEntry;
        PreparedStatement _deleteLedgerEntry;

        /// <inheritdoc/>
        public LedgerEntryRepository(ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger logger) : base(session, serializationFormat, serializationContext, logger)
        {
            _tableName = "ledger";
        }

        /// <inheritdoc/>
        protected override async Task Initialize()
        {
            _getAllLedgers = new SimpleStatement("SELECT etag, format, updated, contents FROM {0}".FormatWith(_tableName));
            _getLedgerEntry = await PrepareAsync("SELECT etag, format, updated, contents FROM {0} WHERE id = ? AND ledgerid = ?".FormatWith(_tableName));
            _getFullLedger = await PrepareAsync("SELECT etag, format, updated, contents FROM {0} WHERE id = ?".FormatWith(_tableName));
            _insertLedgerEntry = await PrepareAsync("INSERT INTO {0} (id, ledgerid, etag, format, updated, contents) VALUES (?, ?, ?, ?, ?, ?) IF NOT EXISTS".FormatWith(_tableName));
            _updateLedgerEntry = await PrepareAsync("UPDATE {0} SET etag=?, updated=?, contents=?, format=? WHERE id=? AND ledgerid=? IF etag=?".FormatWith(_tableName));
            _deleteLedgerEntry = await PrepareAsync("DELETE FROM {0} WHERE id=? AND ledgerid=? IF etag=?".FormatWith(_tableName));
        }

        /// <summary>
        /// Create the table asynchronously
        /// </summary>
        /// <returns></returns>
        public override async Task CreateTableAsync() =>
            // optimize this to happen once later
            await ExecuteAsync("CREATE TABLE IF NOT EXISTS {0} (id text, ledgerid timeuuid, etag text, format text, updated timestamp, contents blob, PRIMARY KEY(id, ledgerid) ) WITH CLUSTERING ORDER BY ( ledgerid DESC );".FormatWith(_tableName), "create_table_" + _tableName);

        /// <inheritdoc/>
        public async Task<ICollection<LedgerEntry>> GetAll()
        {
            try
            {
                var rowSet = await ExecuteAsync(_getAllLedgers, "getAll_" + _tableName);
                return await ReadAsAsync<LedgerEntry>(rowSet);
            }
            catch (DriverException queryError)
            {
                _log.LogError(queryError, "Failed to execute query");
            }

            return new List<LedgerEntry>();
        }

        /// <inheritdoc/>
        public async Task<LedgerEntry> Get(StorageKey id)
        {
            try
            {
                var rowSet = await ExecuteAsync(_getFullLedger.Bind(id.RawValue()), "get_" + _tableName);
                return (await ReadAsAsync<LedgerEntry>(rowSet)).FirstOrDefault();
            }
            catch (DriverException queryError)
            {
                _log.LogError(queryError, "Failed to execute query");
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<ICollection<LedgerEntry>> GetAny(StorageKey id)
        {
            try
            {
                var ledgerId = id as LedgerCompositeEntryKey;
                if(ledgerId != null)
                {
                    var rowSet =ledgerId.LedgerTimeId == null ? await ExecuteAsync(_getFullLedger.Bind(ledgerId.TargetId), "getAny_" + _tableName) :
                        await ExecuteAsync(_getLedgerEntry.Bind(ledgerId.TargetId, ledgerId.LedgerTimeId), "get_" + _tableName);

                    return await ReadAsAsync<LedgerEntry>(rowSet);
                }
            }
            catch (DriverException queryError)
            {
                _log.LogError(queryError, "Failed to execute query");
            }

            return new List<LedgerEntry>();
        }

        /// <inheritdoc/>
        public async Task<bool> TryInsert(LedgerEntry obj)
        {
            try
            {
                _log.LogInformation("Inserting new ledger for {0}", obj.Key);
                var id = obj.Key as LedgerCompositeEntryKey;
                if (id.LedgerTimeId == null)
                    id.LedgerTimeId = TimeUuid.NewId();

                var contents = new byte[0];
                using (var ms = new MemoryStream())
                {
                    await _serializationContext.WriteTo(obj, ms, true, _serializationFormat);
                    ms.Seek(0, SeekOrigin.Begin);
                    contents = ms.ToArray();
                }

                obj.ETag = contents.CalculateETag();

                var rowSet = await ExecuteAsync(_insertLedgerEntry.Bind(id.TargetId, id.LedgerTimeId, obj.ETag, _serializationFormat.ToString(), DateTime.UtcNow, contents), "insert_" + _tableName);

                return rowSet != null;
            }
            catch (DriverException queryError)
            {
                _log.LogError(queryError, "Failed to execute query");
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> TryRemove(LedgerEntry obj)
        {
            try
            {
                var id = obj.Key as LedgerCompositeEntryKey;
                _log.LogInformation("Removing ledger from {0}", id.TargetId);
                var rowSet = await ExecuteAsync(_deleteLedgerEntry.Bind(id.TargetId, id.LedgerTimeId, obj.ETag), "delete_" + _tableName);

                return rowSet != null;
            }
            catch (DriverException queryError)
            {
                _log.LogError(queryError, "Failed to execute query");
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> TryUpdate(LedgerEntry obj)
        {
            var oldEtag = obj.ETag;
            try
            {
                var id = obj.Key as LedgerCompositeEntryKey;
                _log.LogInformation("Updating ledger entry for {0}", id.TargetId);
                var contents = new byte[0];
                using (var ms = new MemoryStream())
                {
                    await _serializationContext.WriteTo(obj, ms, true, _serializationFormat);
                    ms.Seek(0, SeekOrigin.Begin);
                    contents = ms.ToArray();
                }

                obj.ETag = contents.CalculateETag();

                var rowSet = await ExecuteAsync(_updateLedgerEntry.Bind(obj.ETag, DateTime.UtcNow, contents, _serializationFormat.ToString(), id.TargetId, id.LedgerTimeId, oldEtag), "update_" + _tableName);

                return rowSet != null;
            }
            catch (DriverException queryError)
            {
                _log.LogError(queryError, "Failed to execute query");
            }

            return false;
        }
    }
}