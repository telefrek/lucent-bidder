using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class BidEntitySerializer : IEntitySerializer<Bid>
    {
        public Bid Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Bid> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!await serializationStreamReader.HasNextAsync())
                    return null;

            var instance = new Bid();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.Id = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 2:
                                instance.ImpressionId = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 3:
                                instance.CPM = await serializationStreamReader.ReadDoubleAsync();
                                break;
                            case 4:
                                instance.AdId = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 5:
                                instance.WinUrl = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 6:
                                instance.AdMarkup = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 7:
                                instance.AdDomain = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 8:
                                instance.ImageUrl = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 9:
                                instance.CampaignId = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 10:
                                instance.CreativeId = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 11:
                                instance.CreativeAttributes = await serializationStreamReader.ReadAsArrayAsync<CreativeAttribute>();
                                break;
                            case 13:
                                instance.DealId = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 14:
                                instance.Bundle = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 15:
                                instance.ContentCategories = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 16:
                                instance.W = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 17:
                                instance.H = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 18:
                                instance.API = await serializationStreamReader.ReadAsAsync<ApiFramework>();
                                break;
                            case 19:
                                instance.Protocol = await serializationStreamReader.ReadAsAsync<VideoProtocol>();
                                break;
                            case 20:
                                instance.MediaRating = await serializationStreamReader.ReadAsAsync<MediaRating>();
                                break;
                            case 21:
                                instance.BidExpiresSeconds = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 22:
                                instance.BillingUrl = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 23:
                                instance.LossUrl = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 24:
                                instance.TacticId = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 25:
                                instance.Language = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 26:
                                instance.WRatio = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 27:
                                instance.HRatio = await serializationStreamReader.ReadIntAsync();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "id":
                        instance.Id = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "impid":
                        instance.ImpressionId = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "price":
                        instance.CPM = await serializationStreamReader.ReadDoubleAsync();
                        break;
                    case "adid":
                        instance.AdId = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "nurl":
                        instance.WinUrl = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "adm":
                        instance.AdMarkup = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "adomain":
                        instance.AdDomain = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "iurl":
                        instance.ImageUrl = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "cid":
                        instance.CampaignId = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "crid":
                        instance.CreativeId = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "attr":
                        instance.CreativeAttributes = await serializationStreamReader.ReadAsArrayAsync<CreativeAttribute>();
                        break;
                    case "dealid":
                        instance.DealId = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "bundle":
                        instance.Bundle = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "cat":
                        instance.ContentCategories = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "w":
                        instance.W = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "h":
                        instance.H = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "api":
                        instance.API = await serializationStreamReader.ReadAsAsync<ApiFramework>();
                        break;
                    case "protocol":
                        instance.Protocol = await serializationStreamReader.ReadAsAsync<VideoProtocol>();
                        break;
                    case "qagmediarating":
                        instance.MediaRating = await serializationStreamReader.ReadAsAsync<MediaRating>();
                        break;
                    case "exp":
                        instance.BidExpiresSeconds = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "burl":
                        instance.BillingUrl = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "lurl":
                        instance.LossUrl = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "tactic":
                        instance.TacticId = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "language":
                        instance.Language = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "wratio":
                        instance.WRatio = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "hratio":
                        instance.HRatio = await serializationStreamReader.ReadIntAsync();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Bid instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Bid instance, CancellationToken token)
        {
            if (instance != null)
            {
                serializationStreamWriter.StartObject();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "id" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "impid" }, instance.ImpressionId);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "price" }, instance.CPM);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "adid" }, instance.AdId);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "nurl" }, instance.WinUrl);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "adm" }, instance.AdMarkup);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 7, Name = "adomain" }, instance.AdDomain);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 8, Name = "iurl" }, instance.ImageUrl);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 9, Name = "cid" }, instance.CampaignId);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 10, Name = "crid" }, instance.CreativeId);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 11, Name = "attr" }, instance.CreativeAttributes);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 13, Name = "dealid" }, instance.DealId);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 14, Name = "bundle" }, instance.Bundle);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 15, Name = "cat" }, instance.ContentCategories);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 16, Name = "w" }, instance.W);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 17, Name = "h" }, instance.H);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 18, Name = "api" }, instance.API);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 19, Name = "protocol" }, instance.Protocol);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 20, Name = "qagmediarating" }, instance.MediaRating);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 21, Name = "exp" }, instance.BidExpiresSeconds);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 22, Name = "burl" }, instance.BillingUrl);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 23, Name = "lurl" }, instance.LossUrl);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 24, Name = "tactic" }, instance.TacticId);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 25, Name = "language" }, instance.Language);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 26, Name = "wratio" }, instance.WRatio);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 27, Name = "hratio" }, instance.HRatio);
                serializationStreamWriter.EndObject();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}