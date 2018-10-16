using System;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Bidding;
using Lucent.Common.Exchanges;
using Lucent.Common.Messaging;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Samples.SimpleExchange
{
    /// <summary>
    /// Implementation of a very basic exchange
    /// </summary>
    public class SimpleExchange : IAdExchange
    {
        IStorageManager _storageManager;
        IMessageFactory _messageFactory;
        ISerializationRegistry _serializationRegistry;
        IBidFactory _bidFactory;

        public void Initialize(IServiceProvider provider)
        {
            _storageManager = provider.GetService<IStorageManager>();
            _messageFactory = provider.GetService<IMessageFactory>();
            _serializationRegistry = provider.GetService<ISerializationRegistry>();
            _bidFactory = provider.GetService<IBidFactory>();
        }

        /// <inheritdoc/>
        public bool SuppressBOM => true;

        /// <inheritdoc/>
        public string Name => "SimpleExchange";

        /// <inheritdoc/>
        public Guid ExchangeId => Guid.Parse("9363aae4-a305-43e6-b0be-a2f5cda1edff");

        /// <inheritdoc/>
        public Task<BidResponse> Bid(BidRequest request)
        {
            return Task.FromResult(new BidResponse
            {
                NoBidReason = NoBidReason.SuspectedNonHuman,
                Id = request.Id,
                CorrelationId = SequentialGuid.NextGuid().ToString(),
            });
        }

        /// <inheritdoc/>
        public bool IsMatch(HttpContext context)
        {
            return context.Request.Path.ToUriComponent().Contains(Name, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}