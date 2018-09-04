using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class DealEntitySerializer : IEntitySerializer<Deal>
    {
        public Deal Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Deal> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!await serializationStreamReader.HasNextAsync())
                    return null;

            var instance = new Deal();
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
                                instance.BidFloor = await serializationStreamReader.ReadDoubleAsync();
                                break;
                            case 3:
                                instance.BidFloorCur = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 4:
                                instance.WhitelistBuyers = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 5:
                                instance.WhitelistDomains = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 6:
                                instance.AuctionType = await serializationStreamReader.ReadAsAsync<AuctionType>();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "id":
                        instance.Id = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "bidfloor":
                        instance.BidFloor = await serializationStreamReader.ReadDoubleAsync();
                        break;
                    case "bidfloorcur":
                        instance.BidFloorCur = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "wseat":
                        instance.WhitelistBuyers = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "wadomain":
                        instance.WhitelistDomains = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "at":
                        instance.AuctionType = await serializationStreamReader.ReadAsAsync<AuctionType>();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Deal instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Deal instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "id" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "bidfloor" }, instance.BidFloor);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "bidfloorcur" }, instance.BidFloorCur);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "wseat" }, instance.WhitelistBuyers);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "wadomain" }, instance.WhitelistDomains);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "at" }, instance.AuctionType);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}