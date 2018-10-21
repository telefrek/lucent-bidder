using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BidResponseEntitySerializer : IEntitySerializer<BidResponse>
    {
        /// <inheritdoc />
        public BidResponse Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        /// <inheritdoc />
        public async Task<BidResponse> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new BidResponse();
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
                                instance.Bids = await serializationStreamReader.ReadAsArrayAsync<SeatBid>();
                                break;
                            case 3:
                                instance.CorrelationId = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 4:
                                instance.Currency = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 5:
                                instance.CustomData85 = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 6:
                                instance.NoBidReason = await serializationStreamReader.ReadAsAsync<NoBidReason>();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "id":
                        instance.Id = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "seatbid":
                        instance.Bids = await serializationStreamReader.ReadAsArrayAsync<SeatBid>();
                        break;
                    case "bidid":
                        instance.CorrelationId = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "cur":
                        instance.Currency = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "customdata":
                        instance.CustomData85 = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "nbr":
                        instance.NoBidReason = await serializationStreamReader.ReadAsAsync<NoBidReason>();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        /// <inheritdoc />
        public void Write(ISerializationStreamWriter serializationStreamWriter, BidResponse instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        /// <inheritdoc />
        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, BidResponse instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "id" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "seatbid" }, instance.Bids);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "bidid" }, instance.CorrelationId);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "cur" }, instance.Currency);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "customdata" }, instance.CustomData85);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "nbr" }, instance.NoBidReason);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}