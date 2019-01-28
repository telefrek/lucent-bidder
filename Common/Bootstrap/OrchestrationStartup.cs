﻿using Lucent.Common.Entities;
using Lucent.Common.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Prometheus;

namespace Lucent.Common.Bootstrap
{
    /// <summary>
    /// 
    /// </summary>
    public class OrchestrationStartup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMetricServer();
            app.UseMiddleware<MonitoringMiddleware>();
            app.ConfigureHealth();

            app.Map("/api/campaigns", (a) =>
            {
                a.UseMiddleware<EntityRestApi<Campaign>>();
            });

            app.Map("/api/creatives", (a) =>
            {
                a.UseMiddleware<EntityRestApi<Creative>>();
            });

            app.Map("/api/exchanges", (a) =>
            {
                a.UseMiddleware<ExchangeOrchestrator>();
            });

            app.Map("/api/filters", (a) =>
            {
                a.UseMiddleware<EntityRestApi<BidderFilter>>();
            });

            app.Map("/api/budget/request", (a) =>
            {
                a.UseMiddleware<BudgetOrchestrator>();
            });
        }
    }
}