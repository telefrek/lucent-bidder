using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.Entities.Serializers
{
    internal class FilterSerializer<T> : IEntitySerializer<Filter<T>>
    {
        public Filter<T> Read(ISerializationStreamReader serializationStreamReader) => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Filter<T>> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if (serializationStreamReader.Token == SerializationToken.Unknown)
                if (!await serializationStreamReader.HasNextAsync())
                    return null;

            var filter = new Filter<T>();

            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;
                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 0:
                                filter.Property = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 1:
                                filter.FilterType = (FilterType)(await serializationStreamReader.ReadIntAsync());
                                break;
                            case 2:
                                filter.Value = await serializationStreamReader.ReadStringAsync();
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
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;
                }
            }

            // Get the propert
            var prop = typeof(T).GetProperty(filter.Property ?? "", BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.IgnoreCase | BindingFlags.Public);

            // This is a partial hack that will have some edge cases for sure
            if (prop != null && filter.Value != null)
            {
                if (prop.PropertyType.IsEnum)
                    filter.Value = Enum.Parse(prop.PropertyType, filter.Value as string);
                else
                    filter.Value = Convert.ChangeType(filter.Value, prop.PropertyType);
            }

            return filter;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Filter<T> instance) => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Filter<T> instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 0, Name = "property" }, instance.Property);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "type" }, (int)instance.FilterType);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "value" }, instance.Value.ToString());
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}