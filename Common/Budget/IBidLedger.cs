using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// Stores bid history
    /// </summary>
    public interface IBidLedger
    {
        /// <summary>
        /// Tries to record an entry and the source + metadata
        /// </summary>
        /// <param name="ledgerId"></param>
        /// <param name="source"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        Task<bool> TryRecordEntry(string ledgerId, BidEntry source, Dictionary<string, object> metadata);

        /// <summary>
        /// Get the summary over the given start and end dates for the entity, divided into the specified number of segments
        /// </summary>
        /// <param name="entityId">The entity</param>
        /// <param name="start">The start time (inclusive)</param>
        /// <param name="end">The end time (exclusive)</param>
        /// <param name="numSegments">The optional number of segments (Default = 1)</param>
        /// <param name="detailed">Option for including details</param>
        /// <param name="clickOnly"></param>
        /// <returns></returns>
        Task<ICollection<LedgerSummary>> TryGetSummary(string entityId, DateTime start, DateTime end, int? numSegments, bool? detailed, bool? clickOnly);
    }

    /// <summary>
    /// Summary of a given timespan
    /// </summary>
    public class LedgerSummary
    {
        /// <summary>
        /// The start for this block
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "start")]
        public DateTime Start { get; set; }

        /// <summary>
        /// The end of the block
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "end")]
        public DateTime End { get; set; }

        /// <summary>
        /// The total spent during this block
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "amount")]
        public double Amount { get; set; }

        /// <summary>
        /// The number of bids won
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "bids")]
        public int Bids { get; set; }

        /// <summary>
        /// Metadata
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> Metadata { get; set; } = new Dictionary<string, int>();
    }
}