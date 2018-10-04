using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.Filters.Serializers
{
    internal class FilterSerializer : IEntitySerializer<Filter>
    {
        public Filter Read(ISerializationStreamReader serializationStreamReader) => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Filter> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!await serializationStreamReader.HasNextAsync())
                    return null;

            var filter = new Filter();

            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;
                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                filter.Property = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 2:
                                filter.FilterType = (FilterType)(await serializationStreamReader.ReadIntAsync());
                                break;
                            case 3:
                                filter.Value = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 4:
                                filter.PropertyType = Type.GetType(await serializationStreamReader.ReadStringAsync(), true, true);
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "property":
                        filter.Property = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "type":
                        filter.FilterType = (FilterType)(await serializationStreamReader.ReadIntAsync());
                        break;
                    case "value":
                        filter.Value = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "ptype":
                        filter.PropertyType = Type.GetType(await serializationStreamReader.ReadStringAsync(), true, true);
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;
                }
            }

            if (filter.PropertyType.IsEnum)
                filter.Value = Enum.Parse(filter.PropertyType, filter.Value as string);
            else
                filter.Value = Convert.ChangeType(filter.Value, filter.PropertyType);

            return filter;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Filter instance) => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Filter instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "property" }, instance.Property);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "type" }, (int)instance.FilterType);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "value" }, instance.Value.ToString());
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "ptype" }, instance.PropertyType.AssemblyQualifiedName);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}