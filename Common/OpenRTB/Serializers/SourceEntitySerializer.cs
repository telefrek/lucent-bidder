using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class SourceEntitySerializer : IEntitySerializer<Source>
    {
        public Source Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Source> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new Source();
            while (await serializationStreamReader.HasMorePropertiesAsync())
            {
                var propId = serializationStreamReader.Id;

                switch (propId.Name)
                {
                    case "":
                        switch (propId.Id)
                        {
                            case 1:
                                instance.FinalDecision = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 2:
                                instance.TransactionId = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 3:
                                instance.PaymentChain = await serializationStreamReader.ReadStringAsync();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "fd":
                        instance.FinalDecision = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "tid":
                        instance.TransactionId = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "pchain":
                        instance.PaymentChain = await serializationStreamReader.ReadStringAsync();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Source instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Source instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "fd" }, instance.FinalDecision);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "tid" }, instance.TransactionId);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "pchain" }, instance.PaymentChain);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}