using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class UserEntitySerializer : IEntitySerializer<User>
    {
        public User Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<User> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new User();
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
                                instance.BuyerId = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 3:
                                instance.YOB = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 4:
                                instance.Gender = await serializationStreamReader.ReadAsAsync<Gender>();
                                break;
                            case 5:
                                instance.Keywords = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 6:
                                instance.CustomB85 = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 7:
                                instance.Geo = await serializationStreamReader.ReadAsAsync<Geo>();
                                break;
                            case 8:
                                instance.Data = await serializationStreamReader.ReadAsArrayAsync<Data>();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "id":
                        instance.Id = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "buyeruid":
                        instance.BuyerId = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "yob":
                        instance.YOB = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "gender":
                        instance.Gender = await serializationStreamReader.ReadAsAsync<Gender>();
                        break;
                    case "keywords":
                        instance.Keywords = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "customdata":
                        instance.CustomB85 = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "geo":
                        instance.Geo = await serializationStreamReader.ReadAsAsync<Geo>();
                        break;
                    case "data":
                        instance.Data = await serializationStreamReader.ReadAsArrayAsync<Data>();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, User instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, User instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "id" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "buyeruid" }, instance.BuyerId);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "yob" }, instance.YOB);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "gender" }, instance.Gender);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "keywords" }, instance.Keywords);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "customdata" }, instance.CustomB85);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 7, Name = "geo" }, instance.Geo);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 8, Name = "data" }, instance.Data);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}