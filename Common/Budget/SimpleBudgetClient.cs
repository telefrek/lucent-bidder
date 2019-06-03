using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common.Bidding;
using Lucent.Common.Client;
using Lucent.Common.Entities;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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
        String _jwt;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        /// <param name="serializationContext"></param>
        /// <param name="clientManager"></param>
        public SimpleBudgetClient(ILogger<SimpleBudgetClient> logger, IOptions<BudgetConfig> config, ISerializationContext serializationContext, IClientManager clientManager)
        {
            _log = logger;
            _config = config.Value;
            _serializationContext = serializationContext;
            _clientManager = clientManager;

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("please don't use this"));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var tokeOptions = new JwtSecurityToken(
                issuer: "https://lucentbid.com",
                audience: "https://lucentbid.com",
                claims: new List<Claim>(),
                expires: DateTime.Now.AddYears(1),
                signingCredentials: signinCredentials
            );

            _jwt = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            _clientManager.OrchestrationClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public async Task<bool> RequestBudget(string entityId, EntityType entityType)
        {
            var req = new BudgetRequest { EntityId = entityId, CorrelationId = SequentialGuid.NextGuid(), EntityType = entityType };
            using (var ms = new MemoryStream())
            {
                BidCounters.BudgetRequests.WithLabels("request").Inc();
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