using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Dynamic;
using Microsoft.Extensions.Logging;

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
        readonly ILogger _log;

        public RabbitHttpPublisher(IMessageFactory factory, ILogger log, RabbitCluster clusterInfo, string topic)
        {
            _log = log;
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
            using (var ms = new MemoryStream())
            {
                using (var jsw = new JsonTextWriter(new StreamWriter(ms)))
                {
                    dynamic expando = new ExpandoObject();
                    expando.properties = new ExpandoObject();
                    jsw.WritePropertyName("delivery_mode");
                    jsw.WriteValue(2);
                    jsw.WritePropertyName("content_type");
                    jsw.WriteValue(message.ContentType);

                    if (message.Headers != null && message.Headers.Count > 0)
                    {
                        jsw.WritePropertyName("headers");
                        jsw.WriteStartArray();
                        foreach (var header in message.Headers)
                            jsw.WriteValue(header);
                        jsw.WriteEndArray();
                    }

                    jsw.WritePropertyName("routing_key");
                    jsw.WriteValue(message.Route);
                    jsw.WritePropertyName("payload");
                    jsw.WriteValue(Convert.ToBase64String(message.ToBytes()));
                    jsw.WritePropertyName("payload_encoding");
                    jsw.WriteValue("base64");
                    jsw.Flush();

                    var msg = Encoding.UTF8.GetString(ms.ToArray());

                    _log.LogInformation("Sending : {0}", msg);

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
    }
}