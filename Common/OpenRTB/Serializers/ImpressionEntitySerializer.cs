using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class ImpressionEntitySerializer : IEntitySerializer<Impression>
    {
        public Impression Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Impression> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new Impression();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.ImpressionId = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 2:
                                instance.Banner = await serializationStreamReader.ReadAsAsync<Banner>();
                                break;
                            case 3:
                                instance.Video = await serializationStreamReader.ReadAsAsync<Video>();
                                break;
                            case 4:
                                instance.DisplayManager = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 5:
                                instance.DisplayManagerVersion = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 6:
                                instance.FullScreen = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 7:
                                instance.TagId = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 8:
                                instance.BidFloor = await serializationStreamReader.ReadDoubleAsync();
                                break;
                            case 9:
                                instance.BidCurrency = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 10:
                                instance.IFrameBusters = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 11:
                                instance.PrivateMarketplace = await serializationStreamReader.ReadAsAsync<PrivateMarketplace>();
                                break;
                            case 12:
                                instance.IsHttpsRequired = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 13:
                                // No Native support
                                await serializationStreamReader.SkipAsync();
                                break;
                            case 14:
                                instance.ExpectedAuctionDelay = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 15:
                                instance.Audio = await serializationStreamReader.ReadAsAsync<Audio>();
                                break;
                            case 16:
                                instance.IsClickNative = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 17:
                                instance.Metrics = await serializationStreamReader.ReadAsArrayAsync<Metric>();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "id":
                        instance.ImpressionId = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "banner":
                        instance.Banner = await serializationStreamReader.ReadAsAsync<Banner>();
                        break;
                    case "video":
                        instance.Video = await serializationStreamReader.ReadAsAsync<Video>();
                        break;
                    case "displaymanager":
                        instance.DisplayManager = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "displaymanagerver":
                        instance.DisplayManagerVersion = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "instl":
                        instance.FullScreen = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "tagid":
                        instance.TagId = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "bidfloor":
                        instance.BidFloor = await serializationStreamReader.ReadDoubleAsync();
                        break;
                    case "bidfloorcur":
                        instance.BidCurrency = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "iframebuster":
                        instance.IFrameBusters = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "pmp":
                        instance.PrivateMarketplace = await serializationStreamReader.ReadAsAsync<PrivateMarketplace>();
                        break;
                    case "secure":
                        instance.IsHttpsRequired = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "native":
                        // No Native support
                        await serializationStreamReader.SkipAsync();
                        break;
                    case "exp":
                        instance.ExpectedAuctionDelay = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "audio":
                        instance.Audio = await serializationStreamReader.ReadAsAsync<Audio>();
                        break;
                    case "clickbrowser":
                        instance.IsClickNative = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "metric":
                        instance.Metrics = await serializationStreamReader.ReadAsArrayAsync<Metric>();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Impression instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Impression instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "id" }, instance.ImpressionId);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "banner" }, instance.Banner);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "video" }, instance.Video);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "displaymanager" }, instance.DisplayManager);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "displaymanagerver" }, instance.DisplayManagerVersion);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "instl" }, instance.FullScreen);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 7, Name = "tagid" }, instance.TagId);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 8, Name = "bidfloor" }, instance.BidFloor);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 9, Name = "bidfloorcur" }, instance.BidCurrency);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 10, Name = "iframebuster" }, instance.IFrameBusters);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 11, Name = "pmp" }, instance.PrivateMarketplace);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 12, Name = "secure" }, instance.IsHttpsRequired);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 14, Name = "exp" }, instance.ExpectedAuctionDelay);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 15, Name = "audio" }, instance.Audio);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 16, Name = "clickbrowser" }, instance.IsClickNative);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 17, Name = "metric" }, instance.Metrics);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}