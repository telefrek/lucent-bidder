using System;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Lucent.Common.Caching
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBudgetCache
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="allocation"></param>
        /// <returns></returns>
        Task<BudgetStatus> TryUpdateBudget(BudgetAllocation allocation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<BudgetStatus> TryUpdateSpend(string key, double value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<BudgetStatus> TryGetRemaining(string key);
    }

    /// <summary>
    /// Simple status
    /// </summary>
    public class BudgetStatus
    {
        /// <summary>
        /// Call success
        /// </summary>
        /// <value></value>
        public bool Successful { get; set; }

        /// <summary>
        /// Remaining amount after op
        /// </summary>
        /// <value></value>
        public double Remaining { get; set; }

        /// <summary>
        /// Amount spend since last reset
        /// </summary>
        /// <value></value>
        public double Spend { get; set; }

        /// <summary>
        /// The daily amount spent
        /// </summary>
        /// <value></value>
        public double TotalSpend { get; set; }

        /// <summary>
        /// Get the last hour
        /// </summary>
        /// <value></value>
        public DateTime LastUpdate { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BudgetAllocation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double Amount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool ResetSpend { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool ResetDaily { get; set; }
    }

    /// <summary>
    /// In memory shim
    /// </summary>
    public class InMemoryBudgetCache : IBudgetCache
    {
        static readonly IMemoryCache _memcache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromSeconds(15) });

        /// <inheritdoc/>
        public Task<BudgetStatus> TryGetRemaining(string key)
        {
            dynamic obj = _memcache.Get(key);
            var status = new BudgetStatus();
            status.Successful = true;

            if (obj != null)
            {
                status.Remaining = obj.budget;
                status.LastUpdate = obj.updated;
                status.Spend = obj.spend;
                status.TotalSpend = obj.total;
            }
            else
            {
                status.Remaining = 0;
                status.LastUpdate = DateTime.UtcNow.AddDays(-1);
                status.Spend = 0;
                status.TotalSpend = 0;
            }

            return Task.FromResult(status);
        }

        /// <inheritdoc/>
        public Task<BudgetStatus> TryUpdateBudget(BudgetAllocation allocation)
        {
            var status = new BudgetStatus();
            dynamic obj = _memcache.Get(allocation.Key);
            if (obj == null)
            {
                obj = new ExpandoObject();
                obj.budget = 0;
                obj.spend = 0;
                obj.total = 0;
                obj.updated = DateTime.UtcNow.AddDays(-1);
            }

            obj.budget += allocation.Amount;

            if(allocation.ResetSpend)
                obj.spend = 0;
            if (allocation.ResetDaily)
                obj.daily = 0;

            obj.updated = DateTime.UtcNow;

            _memcache.Set(allocation.Key, (object)obj, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(60),
            });

            status.Successful = true;
            status.Spend = obj.spend;
            status.TotalSpend = obj.total;
            status.Remaining = obj.budget - obj.spend;
            status.LastUpdate = obj.updated;

            return Task.FromResult(status);
        }

        /// <inheritdoc/>
        public Task<BudgetStatus> TryUpdateSpend(string key, double value)
        {
            var status = new BudgetStatus();
            dynamic obj = _memcache.Get(key);
            if (obj == null)
            {
                obj = new ExpandoObject();
                obj.budget = 0;
                obj.spend = 0;
                obj.total = 0;
                obj.updated = DateTime.UtcNow.AddDays(-1);
            }

            obj.spend += value;
            obj.total += value;
            obj.budget -= value;

            _memcache.Set(key, (object)obj, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(60),
            });

            status.Successful = true;
            status.TotalSpend = obj.total;
            status.Remaining = obj.budget - obj.spend;
            status.Spend = obj.spend;
            status.LastUpdate = obj.updated;

            return Task.FromResult(status);
        }
    }
}