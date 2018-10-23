using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SeatBidEntitySerializer : IEntitySerializer<SeatBid>
    {
        /// <inheritdoc />
        public SeatBid Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        /// <inheritdoc />
        public async Task<SeatBid> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new SeatBid();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.Bids = await serializationStreamReader.ReadAsArrayAsync<Bid>();
                                break;
                            case 2:
                                instance.BuyerId = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 3:
                                instance.IsGrouped = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "bid":
                        instance.Bids = await serializationStreamReader.ReadAsArrayAsync<Bid>();
                        break;
                    case "seat":
                        instance.BuyerId = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "group":
                        instance.IsGrouped = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            
            if(!await serializationStreamReader.EndObjectAsync())
                return null;
                
            return instance;
        }

        /// <inheritdoc />
        public void Write(ISerializationStreamWriter serializationStreamWriter, SeatBid instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        /// <inheritdoc />
        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, SeatBid instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "bid" }, instance.Bids);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "seat" }, instance.BuyerId);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "group" }, instance.IsGrouped);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}