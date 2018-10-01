using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Lucent.Common.Serialization.Json;
using System.Dynamic;
using System.Collections.Generic;

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
            using (var ms = new MemoryStream())
            {
                using (var jsw = new JsonSerializationStreamWriter(new JsonTextWriter(new StreamWriter(ms)), null, null))
                {

                    dynamic expando = new ExpandoObject();
                    expando.properties = new ExpandoObject();
                    expando.properties.delivery_mode = 2;
                    expando.properties.content_type = message.ContentType;

                    if (message.Headers != null && message.Headers.Count > 0)
                    {
                        expando.properties.headers = new ExpandoObject();
                        foreach (var header in message.Headers)
                            ((IDictionary<string, object>)expando.properties.headers).Add(header);
                    }

                    expando.routing_key = message.Route;
                    expando.payload = Convert.ToBase64String(message.ToBytes());
                    expando.payload_encoding = "base64";

                    jsw.Write(expando);
                    jsw.Flush();

                    var msg = Encoding.UTF8.GetString(ms.ToArray());

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