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
    public class LedgerEntryRepository : CassandraBaseRepository, IStorageRepository<LedgerEntry, LedgerCompositeEntryKey>
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
        protected override void Initialize()
        {
            _getAllLedgers = new SimpleStatement("SELECT etag, format, updated, contents FROM {0}".FormatWith(_tableName));
            _getLedgerEntry = Prepare("SELECT etag, format, updated, contents FROM {0} WHERE id = ? AND ledgerid = ?".FormatWith(_tableName));
            _getFullLedger = Prepare("SELECT etag, format, updated, contents FROM {0} WHERE id = ?".FormatWith(_tableName));
            _insertLedgerEntry = Prepare("INSERT INTO {0} (id, ledgerid, etag, format, updated, contents) VALUES (?, ?, ?, ?, ?, ?) IF NOT EXISTS".FormatWith(_tableName));
            _updateLedgerEntry = Prepare("UPDATE {0} SET etag=?, updated=?, contents=?, format=? WHERE id=? AND ledgerid=? IF etag=?".FormatWith(_tableName));
            _deleteLedgerEntry = Prepare("DELETE FROM {0} WHERE id=? AND ledgerid=? IF etag=?".FormatWith(_tableName));
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
                var rowSet = await ExecuteAsync(_getAllLedgers, "getAll_ledger");

                return await ReadAsAsync(rowSet, (row) =>
                    {
                        var contents = row.GetValue<byte[]>("contents");
                        var format = Enum.Parse<SerializationFormat>(row.GetValue<string>("format"));

                        using (var ms = new MemoryStream(contents))
                        {
                            using (var reader = _serializationContext.CreateReader(ms, false, _serializationFormat))
                            {
                                if (reader.HasNext())
                                {
                                    var o = reader.ReadAs<LedgerEntry>();
                                    o.ETag = row.GetValue<string>("etag");
                                    o.Updated = row.GetValue<DateTime>("updated");
                                    return o;
                                }
                            }
                        }

                        return null;
                    });
            }
            catch (InvalidQueryException queryError)
            {
                _log.LogError(queryError, "Checking for table missing error");

                if ((queryError.Message ?? "").ToLowerInvariant().Contains("columnfamily") ||
                (queryError.Message ?? "").ToLowerInvariant().Contains("table"))
                    // Recreate
                    await CreateTableAsync();
            }

            return new List<LedgerEntry>();
        }

        /// <inheritdoc/>
        public async Task<LedgerEntry> Get(LedgerCompositeEntryKey id)
        {
            try
            {
                var rowSet = await ExecuteAsync(_getFullLedger.Bind(id.TargetId, id.LedgerTimeId), "get_ledger");

                return (await ReadAsAsync(rowSet, (row) =>
                    {
                        var contents = row.GetValue<byte[]>("contents");
                        var format = Enum.Parse<SerializationFormat>(row.GetValue<string>("format"));

                        using (var ms = new MemoryStream(contents))
                        {
                            using (var reader = _serializationContext.CreateReader(ms, false, _serializationFormat))
                            {
                                if (reader.HasNext())
                                {
                                    var o = reader.ReadAs<LedgerEntry>();
                                    o.ETag = row.GetValue<string>("etag");
                                    o.Updated = row.GetValue<DateTime>("updated");
                                    return o;
                                }
                            }
                        }

                        return null;
                    })).FirstOrDefault();
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
        public async Task<ICollection<LedgerEntry>> GetAny(LedgerCompositeEntryKey id)
        {
            try
            {
                var rowSet = id.LedgerTimeId == null ? await ExecuteAsync(_getFullLedger.Bind(id.TargetId), "getAny_ledger") :
                    await ExecuteAsync(_getLedgerEntry.Bind(id.TargetId, id.LedgerTimeId), "get_ledger");

                return await ReadAsAsync(rowSet, (row) =>
                    {
                        var contents = row.GetValue<byte[]>("contents");
                        var format = Enum.Parse<SerializationFormat>(row.GetValue<string>("format"));

                        using (var ms = new MemoryStream(contents))
                        {
                            using (var reader = _serializationContext.CreateReader(ms, false, _serializationFormat))
                            {
                                if (reader.HasNext())
                                {
                                    var o = reader.ReadAs<LedgerEntry>();
                                    o.ETag = row.GetValue<string>("etag");
                                    o.Updated = row.GetValue<DateTime>("updated");
                                    return o;
                                }
                            }
                        }

                        return null;
                    });
            }
            catch (InvalidQueryException queryError)
            {
                _log.LogError(queryError, "Checking for table missing error");

                if ((queryError.Message ?? "").ToLowerInvariant().Contains("columnfamily") ||
                (queryError.Message ?? "").ToLowerInvariant().Contains("table"))
                    // Recreate
                    await CreateTableAsync();
            }

            return new List<LedgerEntry>();
        }

        /// <inheritdoc/>
        public async Task<bool> TryInsert(LedgerEntry obj)
        {
            try
            {
                _log.LogInformation("Inserting new ledger for {0}", obj.Id.TargetId);
                if (obj.Id.LedgerTimeId == null)
                    obj.Id.LedgerTimeId = TimeUuid.NewId();

                var contents = new byte[0];
                using (var ms = new MemoryStream())
                {
                    using (var writer = _serializationContext.CreateWriter(ms, true, _serializationFormat))
                    {
                        writer.Write(obj);
                        writer.Flush();
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    contents = ms.ToArray();
                }

                obj.ETag = contents.CalculateETag();

                var rowSet = await ExecuteAsync(_insertLedgerEntry.Bind(obj.Id.TargetId, obj.Id.LedgerTimeId, obj.ETag, _serializationFormat.ToString(), DateTime.UtcNow, contents), "insert_leger");

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
        public async Task<bool> TryRemove(LedgerEntry obj)
        {
            try
            {
                _log.LogInformation("Removing ledger from {0}", obj.Id.TargetId);
                var rowSet = await ExecuteAsync(_deleteLedgerEntry.Bind(obj.Id.TargetId, obj.Id.LedgerTimeId, obj.ETag), "delete_ledger");

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
        public async Task<bool> TryUpdate(LedgerEntry obj)
        {
            var oldEtag = obj.ETag;
            try
            {
                _log.LogInformation("Updating ledger entry for {0}", obj.Id.TargetId);
                var contents = new byte[0];
                using (var ms = new MemoryStream())
                {
                    using (var writer = _serializationContext.CreateWriter(ms, true, _serializationFormat))
                    {
                        writer.Write(obj);
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    contents = ms.ToArray();
                }

                obj.ETag = contents.CalculateETag();

                var rowSet = await ExecuteAsync(_updateLedgerEntry.Bind(obj.ETag, DateTime.UtcNow, contents, _serializationFormat.ToString(), obj.Id.TargetId, obj.Id.LedgerTimeId, oldEtag), "update_" + _tableName);

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