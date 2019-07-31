using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aerospike.Client;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Lucent.Common.Caching
{
    /// <summary>
    /// 
    /// </summary>
    public class BudgetCache : IBudgetCache
    {
        ILogger _log;
        ISerializationContext _serializationContext;
        object _budgetSync = new object();

        /// <summary>
        /// Injection constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serializationContext"></param>
        public BudgetCache(ILogger<BudgetCache> logger, ISerializationContext serializationContext)
        {
            _log = logger;
            _serializationContext = serializationContext;
        }

        /// <inheritdoc/>
        public async Task<BudgetStatus> TryUpdateBudget(BudgetAllocation allocation)
        {
            var status = new BudgetStatus();
            var iValue = (long)Math.Floor(allocation.Amount * 10000);
            var updated = DateTime.UtcNow.ToFileTimeUtc();

            try
            {
                var ops = new List<Operation>();
                ops.Add(Operation.Add(new Bin("budget", iValue)));
                ops.Add(Operation.Put(new Bin("updated", updated)));

                if (allocation.ResetDaily)
                {
                    ops.Add(Operation.Put(new Bin("total", 0L)));
                    ops.Add(Operation.Put(new Bin("daily", updated)));

                    ops.Add(Operation.Put(new Bin("spend", 0L)));
                    ops.Add(Operation.Put(new Bin("hourly", updated)));
                }
                else if (allocation.ResetSpend)
                {
                    ops.Add(Operation.Put(new Bin("spend", 0L)));
                    ops.Add(Operation.Put(new Bin("hourly", updated)));
                }

                ops.Add(Operation.Get());

                using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "lucent", "update_budget"))
                {
                    var res = await Aerospike.INSTANCE.Operate(new WritePolicy(Aerospike.INSTANCE.writePolicyDefault) { expiration = 86400 }, default(CancellationToken), new Key("lucent", "budget", allocation.Key), ops.ToArray());

                    // Validate the return
                    if (res != null)
                        return FromResult(res);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "failed to update budget for {0}", allocation.Key);
            }

            return status;
        }

        BudgetStatus FromResult(Record record)
        {
            var status = new BudgetStatus();
            status.Successful = true;
            status.LastUpdate = DateTime.UtcNow.AddDays(-1);

            if (record.bins.ContainsKey("total"))
                status.TotalSpend = record.GetLong("total") / 10000d;
            if (record.bins.ContainsKey("spend"))
                status.Spend = record.GetLong("spend") / 10000d;
            if (record.bins.ContainsKey("updated"))
                status.LastUpdate = DateTime.FromFileTimeUtc(record.GetLong("updated"));
            if (record.bins.ContainsKey("budget"))
                status.Remaining = record.GetLong("budget") / 10000d;

            if (record.bins.ContainsKey("hourly"))
                status.LastHourlyRollover = DateTime.FromFileTimeUtc(record.GetLong("hourly"));
            else
                status.LastHourlyRollover = status.LastUpdate;

            if (record.bins.ContainsKey("daily"))
                status.LastDailyRollover = DateTime.FromFileTimeUtc(record.GetLong("daily"));
            else
                status.LastDailyRollover = status.LastHourlyRollover;

            return status;
        }

        /// <inheritdoc/>
        public async Task<BudgetStatus> TryUpdateSpend(string key, double value)
        {
            var status = new BudgetStatus();
            var iValue = (long)Math.Floor(value * 10000);

            try
            {
                using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "lucent", "update_spend"))
                {
                    var res = await Aerospike.INSTANCE.Operate(new WritePolicy(Aerospike.INSTANCE.writePolicyDefault) { expiration = 86400 }, default(CancellationToken), new Key("lucent", "budget", key),
                        Operation.Add(new Bin("spend", iValue)),
                        Operation.Add(new Bin("total", iValue)),
                        Operation.Add(new Bin("budget", -iValue)),
                        Operation.Get());

                    if (res != null)
                        return FromResult(res);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "failed to update spend for {0}", key);
            }

            return status;
        }

        /// <inheritdoc/>
        public async Task<BudgetStatus> TryGetRemaining(string key)
        {
            var status = new BudgetStatus();

            try
            {
                using (var ctx = StorageCounters.LatencyHistogram.CreateContext("aerospike", "lucent", "get_budget"))
                {
                    var res = await Aerospike.INSTANCE.Get(null, default(CancellationToken), new Key("lucent", "budget", key));

                    if (res != null)
                        return FromResult(res);

                    // Doesn't exist, need to update
                    status.Successful = true;
                    status.LastUpdate = DateTime.UtcNow.AddDays(-1);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "failed to get budget for {0}", key);
            }

            return status;
        }
    }
}