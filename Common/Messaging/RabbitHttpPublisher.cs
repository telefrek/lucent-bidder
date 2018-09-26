using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Lucent.Common.Messaging
{
    internal class RabbitHttpPublisher : IMessagePublisher
    {
        static readonly HttpClient _client;

        static RabbitHttpPublisher()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            _client = new HttpClient();
        }

        Uri _publishEndpoint;
        RabbitCluster _cluster;

        public RabbitHttpPublisher(IMessageFactory factory, RabbitCluster clusterInfo, string topic)
        {
            _publishEndpoint = new UriBuilder
            {
                Scheme = "https",
                Host = clusterInfo.Host,
                Path = "/api/exchanges/%2f/{0}/publish".FormatWith(topic)
            }.Uri;
            _cluster = clusterInfo;

            Topic = topic;
        }

        public string Topic { get; set; }

        public void Dispose()
        {
        }

        public bool TryPublish(IMessage message)
        {
            var msg = @"{{""properties"":{{""delivery_mode"":2,""content_type"":""{2}""}},""routing_key"":""{0}"",""payload"":""{1}"",""payload_encoding"":""base64""}}".FormatWith(message.Route, Convert.ToBase64String(message.ToBytes()), message.ContentType);

            using (var req = new HttpRequestMessage(HttpMethod.Post, _publishEndpoint.ToString()))
            {
                req.Content = new StringContent(msg, Encoding.UTF8, "application/json");
                req.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("{0}:{1}".FormatWith(_cluster.User, _cluster.Credentials))));
                var response = _client.SendAsync(req).Result;

                return response.IsSuccessStatusCode;
            }
        }
    }
}