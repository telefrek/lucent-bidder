using System;
using System.Net.Http;

namespace Lucent.Common.Client
{
    /// <summary>
    /// Hack
    /// </summary>
    public class DefaultClientManager : IClientManager
    {
        static readonly HttpClient _orchestrationClient = new HttpClient() { BaseAddress = new Uri("http://orchestrator.lucent.svc") };

        /// <inheritdoc/>
        public HttpClient OrchestrationClient => _orchestrationClient;
    }
}