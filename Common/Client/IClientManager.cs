using System.Net.Http;

namespace Lucent.Common.Client
{
    /// <summary>
    /// Allow injection and lifetime control
    /// </summary>
    public interface IClientManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        HttpClient OrchestrationClient { get; }
    }
}