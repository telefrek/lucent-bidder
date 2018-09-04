using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class BidRequestEntitySerializer : IEntitySerializer<BidRequest>
    {
        public BidRequest Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<BidRequest> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!await serializationStreamReader.HasNextAsync())
                    return null;

            var instance = new BidRequest();
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
                                instance.Impressions = await serializationStreamReader.ReadAsArrayAsync<Impression>();
                                break;
                            case 3:
                                instance.Site = await serializationStreamReader.ReadAsAsync<Site>();
                                break;
                            case 4:
                                instance.App = await serializationStreamReader.ReadAsAsync<App>();
                                break;
                            case 5:
                                instance.Device = await serializationStreamReader.ReadAsAsync<Device>();
                                break;
                            case 6:
                                instance.User = await serializationStreamReader.ReadAsAsync<User>();
                                break;
                            case 7:
                                instance.AuctionType = await serializationStreamReader.ReadAsAsync<AuctionType>();
                                break;
                            case 8:
                                instance.Milliseconds = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 9:
                                instance.WhitelistBuyers = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 10:
                                instance.AllImpressions = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 11:
                                instance.Currencies = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 12:
                                instance.BlockedCategories = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 13:
                                instance.BlockedAdvertisers = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 14:
                                instance.Regulations = await serializationStreamReader.ReadAsAsync<Regulation>();
                                break;
                            case 15:
                                instance.TestFlag = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 16:
                                instance.BlockedApplications = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 17:
                                instance.BlockedBuyers = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 18:
                                instance.Languages = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 19:
                                instance.Source = await serializationStreamReader.ReadAsAsync<Source>();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;

                    case "id":
                        instance.Id = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "imp":
                        instance.Impressions = await serializationStreamReader.ReadAsArrayAsync<Impression>();
                        break;
                    case "site":
                        instance.Site = await serializationStreamReader.ReadAsAsync<Site>();
                        break;
                    case "app":
                        instance.App = await serializationStreamReader.ReadAsAsync<App>();
                        break;
                    case "device":
                        instance.Device = await serializationStreamReader.ReadAsAsync<Device>();
                        break;
                    case "user":
                        instance.User = await serializationStreamReader.ReadAsAsync<User>();
                        break;
                    case "at":
                        instance.AuctionType = await serializationStreamReader.ReadAsAsync<AuctionType>();
                        break;
                    case "tmax":
                        instance.Milliseconds = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "wseat":
                        instance.WhitelistBuyers = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "allimps":
                        instance.AllImpressions = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "cur":
                        instance.Currencies = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "bcat":
                        instance.BlockedCategories = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "badv":
                        instance.BlockedAdvertisers = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "regs":
                        instance.Regulations = await serializationStreamReader.ReadAsAsync<Regulation>();
                        break;
                    case "test":
                        instance.TestFlag = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "bapp":
                        instance.BlockedApplications = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "bseat":
                        instance.BlockedBuyers = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "wlang":
                        instance.Languages = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "source":
                        instance.Source = await serializationStreamReader.ReadAsAsync<Source>();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, BidRequest instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, BidRequest instance, CancellationToken token)
        {
            if (instance != null)
            {
                serializationStreamWriter.StartObject();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "id" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "imp" }, instance.Impressions);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "site" }, instance.Site);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "app" }, instance.App);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "device" }, instance.Device);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "user" }, instance.User);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 7, Name = "at" }, instance.AuctionType);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 8, Name = "tmax" }, instance.Milliseconds);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 9, Name = "wseat" }, instance.WhitelistBuyers);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 10, Name = "allimps" }, instance.AllImpressions);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 11, Name = "cur" }, instance.Currencies);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 12, Name = "bcat" }, instance.BlockedCategories);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 13, Name = "badv" }, instance.BlockedAdvertisers);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 14, Name = "regs" }, instance.Regulations);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 15, Name = "test" }, instance.TestFlag);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 16, Name = "bapp" }, instance.BlockedApplications);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 17, Name = "bseat" }, instance.BlockedBuyers);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 18, Name = "wlang" }, instance.Languages);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 19, Name = "source" }, instance.Source);
                serializationStreamWriter.EndObject();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}