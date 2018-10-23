using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class MetricEntitySerializer : IEntitySerializer<Metric>
    {
        /// <inheritdoc />
        public Metric Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        /// <inheritdoc />
        public async Task<Metric> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
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
            
            if(!await serializationStreamReader.EndObjectAsync())
                return null;
                
            return instance;
        }

        /// <inheritdoc />
        public void Write(ISerializationStreamWriter serializationStreamWriter, Metric instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        /// <inheritdoc />
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