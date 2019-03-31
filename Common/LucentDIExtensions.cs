using Lucent.Common.Messaging;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Lucent.Common.Bidding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lucent.Common.Exchanges;
using Lucent.Common.Entities;
using Lucent.Common.Media;
using Lucent.Common.Entities.Repositories;
using System;
using Lucent.Common.Budget;
using Lucent.Common.Client;
using Microsoft.Extensions.Caching.Memory;
using Lucent.Common.Caching;
using Lucent.Common.Scoring;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;

namespace Lucent.Common
{
    /// <summary>
    /// Dependency Injection extensions
    /// </summary>
    public static partial class LucentDIExtensions
    {
        /// <summary>
        /// Sets up the bidder
        /// </summary>
        /// <param name="services">The current services collection</param>
        /// <param name="configuration">The current configuration</param>
        /// <param name="localOnly">Indicate if this is a setup for local resources only</param>
        /// <param name="includePortal">Flag for portal servicees</param>
        /// <param name="includeBidder">Flag for bidder services</param>
        /// <param name="includeOrchestration">Flag for orchestartion services</param>
        /// <param name="includeScoring">Flag for scoring services</param>
        /// <returns>A modified set of services</returns>
        public static IServiceCollection AddLucentServices(this IServiceCollection services, IConfiguration configuration, bool localOnly = false, bool includePortal = false, bool includeBidder = false, bool includeOrchestration = false, bool includeScoring = false)
        {
            // Enable routing
            services.AddRouting();

            // Enable compression
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes =
                    ResponseCompressionDefaults.MimeTypes.Concat(
                        new[] { "application/json" });
            });

            // Add serialization
            services.AddSingleton<ISerializationContext, LucentSerializationContext>();
            services.AddTransient<IClientManager, DefaultClientManager>();

            // Setup storage, messaging options for local vs distributed cluster
            if (localOnly)
            {
                services.AddSingleton<IAerospikeCache, MockAspkCache>()
                .AddSingleton<IBudgetCache, BudgetCache>();
                services.AddSingleton<IStorageManager, InMemoryStorage>();
                services.AddSingleton<IMessageFactory, InMemoryMessageFactory>();
                services.AddSingleton<IBidLedger, MemoryBudgetLedger>();
            }
            else
            {
                // TODO: Update this to use redis
                services.Configure<AerospikeConfig>(configuration.GetSection("aerospike"))
                    .AddSingleton<IAerospikeCache, AerospikeCache>()
                    .AddSingleton<IBudgetCache, BudgetCache>();

                // Setup storage
                services.Configure<CassandraConfiguration>(configuration.GetSection("cassandra"))
                    .AddSingleton<IStorageManager, CassandraStorageManager>();

                var storageManager = services.BuildServiceProvider().GetRequiredService<IStorageManager>();

                // Register custom repositories
                storageManager.RegisterRepository<ExchangeEntityRespositry, Exchange>();

                services.Configure<RabbitConfiguration>(configuration.GetSection("rabbit"))
                    .AddSingleton<IMessageFactory, RabbitFactory>();
                services.AddSingleton<IBidLedger>(provider =>
                {
                    using (var scope = provider.CreateScope())
                    {
                        var sm = scope.ServiceProvider.GetRequiredService<IStorageManager>();
                        var ledger = (BidLedger)scope.ServiceProvider.CreateInstance<BidLedger>((sm as CassandraStorageManager).Session, SerializationFormat.PROTOBUF);
                        ledger.Initialize(provider).Wait();
                        return ledger;
                    }
                });
            }

            services.AddSingleton<IEntityWatcher, EntityWatcher>();

            if (includePortal)
            {
                services.Configure<MediaConfig>(configuration.GetSection("media"))
                    .AddSingleton<IMediaScanner, MediaScanner>();
            }

            if (includeBidder)
            {
                // Setup bidder
                services
                    .AddSingleton<IScoringService, RandomScoring>()
                    .Configure<BudgetConfig>(configuration.GetSection(Topics.BUDGET))
                    .AddSingleton<IBudgetClient, SimpleBudgetClient>()
                    .AddSingleton<IBudgetManager, SimpleBudgetManager>()
                    .AddTransient<IBiddingManager, BiddingManager>()
                    .AddSingleton<IBidFactory, BidFactory>()
                    .AddSingleton<IExchangeRegistry, ExchangeRegistry>();
            }

            if (includeOrchestration)
            {
                services.Configure<MediaConfig>(configuration.GetSection("media"))
                    .AddSingleton<IMediaScanner, MediaScanner>();
            }

            if (includeScoring)
            {

            }

            return services;
        }
    }
}