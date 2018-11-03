using System.Collections.Generic;
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
        /// <inheritdoc/>
        public LedgerEntryRepository(ISession session, SerializationFormat serializationFormat, ISerializationContext serializationContext, ILogger logger) : base(session, serializationFormat, serializationContext, logger)
        {
        }

        /// <inheritdoc/>
        protected override void Initialize()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<ICollection<LedgerEntry>> GetAll()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<LedgerEntry> Get(LedgerCompositeEntryKey id)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<ICollection<LedgerEntry>> GetAny(LedgerCompositeEntryKey id)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> TryInsert(LedgerEntry obj)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> TryRemove(LedgerEntry obj)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> TryUpdate(LedgerEntry obj)
        {
            throw new System.NotImplementedException();
        }
    }
}