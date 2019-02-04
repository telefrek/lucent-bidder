using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Lucent.Common.Client;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lucent.Common.Budget
{
    /// <summary>
    /// Basic implementation
    /// </summary>
    public class SimpleBudgetClient : IBudgetClient
    {
        ILogger<SimpleBudgetClient> _log;
        BudgetConfig _config;
        ISerializationContext _serializationContext;
        IClientManager _clientManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        /// <param name="serializationContext"></param>
        /// <param name="clientManager"></param>
        public SimpleBudgetClient(ILogger<SimpleBudgetClient> logger, IOptionsSnapshot<BudgetConfig> config, ISerializationContext serializationContext, IClientManager clientManager)
        {
            _log = logger;
            _config = config.Value;
            _serializationContext = serializationContext;
            _clientManager = clientManager;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="amount"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public async Task<bool> RequestBudget(string entityId, double amount, Guid correlationId)
        {
            var req = new BudgetRequest { EntityId = entityId, Amount = amount, CorrelationId = correlationId };
            using (var ms = new MemoryStream())
            {
                await _serializationContext.WriteTo(req, ms, true, SerializationFormat.JSON);
                ms.Seek(0, SeekOrigin.Begin);

                using (var content = new StreamContent(ms, 4092))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var res = await _clientManager.OrchestrationClient.PostAsync("/api/budget/request", content);
                    if (res.StatusCode != HttpStatusCode.Accepted)
                        _log.LogWarning("Failed to retrieve budget for {0} : {1}", entityId, res.StatusCode);
                        
                    return res.StatusCode == HttpStatusCode.Accepted;
                }
            }
        }
    }
}