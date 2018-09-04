using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class MetricEntitySerializer : IEntitySerializer<Metric>
    {
        public Metric Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Metric> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!await serializationStreamReader.HasNextAsync())
                    return null;

            var instance = new Metric();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.MetricType = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 2:
                                instance.Value = await serializationStreamReader.ReadDoubleAsync();
                                break;
                            case 3:
                                instance.Vendor = await serializationStreamReader.ReadStringAsync();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "type":
                        instance.MetricType = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "value":
                        instance.Value = await serializationStreamReader.ReadDoubleAsync();
                        break;
                    case "vendor":
                        instance.Vendor = await serializationStreamReader.ReadStringAsync();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Metric instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Metric instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "type" }, instance.MetricType);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "value" }, instance.Value);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "vendor" }, instance.Vendor);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}