using System.Net.Http;

namespace Lucent.Common.Client
{
    /// <summary>
    /// Hack
    /// </summary>
    public class DefaultClientManager : IClientManager
    {
        static readonly HttpClient _client = new HttpClient();

        /// <inheritdoc/>
        public HttpClient OrchestrationClient => _client;
    }
}