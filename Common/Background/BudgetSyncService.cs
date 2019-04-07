using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Budget;
using Lucent.Common.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Background
{
    /// <summary>
    /// Service that synchronizes budget information
    /// </summary>
    public class BudgetSyncService : IHostedService
    {
        readonly IServiceProvider _serviceProvider;
        volatile bool _isStopped;
        ILogger<BudgetSyncService> _log;
        Task _timerTask;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="log"></param>
        public BudgetSyncService(IServiceProvider serviceProvider, ILogger<BudgetSyncService> log)
        {
            _serviceProvider = serviceProvider;
            _log = log;
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("Starting");

            _timerTask = Task.Factory.StartNew(async () =>
            {
                var bidCache = _serviceProvider.GetRequiredService<IBudgetCache>();
                while (!_isStopped)
                {
                    await Task.Delay(2000);
                    foreach (var budget in LocalBudget.GetAll())
                        try
                        {
                            var current = budget.Collect();
                            var next = await bidCache.TryUpdateBudget(budget.Id, current);
                            if (next == double.NaN)
                                budget.Update(current);
                            else
                                budget.Last = next;
                        }
                        catch (Exception e)
                        {
                            _log.LogError(e, "Error updating {0}", budget.Id);
                        }
                }
            }).Unwrap();

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _isStopped = true;
            await _timerTask;
        }
    }
}