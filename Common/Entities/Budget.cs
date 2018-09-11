using System;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// Tracking for a campaign budget
    /// </summary>
    public class Budget : IStorageEntity
    {
        public string Id { get; set; }
        public string ETag { get; set; }
        public DateTime Updated { get; set; }
        public int Version { get; set; } = 0;
        public double Remaining { get; set; } = 0d;
    }

    /// <summary>
    /// Class to represent budget allocations
    /// </summary>
    public class BudgetLedger : IStorageEntity
    {
        public string Id { get; set; }
        public string ETag { get; set; }
        public DateTime Updated { get; set; }
        public int Version { get; set; } = 0;
        public DateTime LastAllocation { get; set; } = DateTime.Now;
        public double HourlyTotal { get; set; } = 0d;
        public double DailyTotal { get; set; } = 0d;
        public double LifetimeTotal { get; set; } = 0d;
    }
}