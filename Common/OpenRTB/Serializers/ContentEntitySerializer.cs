using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    public sealed class ContentEntitySerializer : IEntitySerializer<Content>
    {
        public Content Read(ISerializationStreamReader serializationStreamReader)
            => ReadAsync(serializationStreamReader, CancellationToken.None).Result;

        public async Task<Content> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            if(!await serializationStreamReader.StartObjectAsync())
                return null;

            var instance = new Content();
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
                                instance.Episode = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 3:
                                instance.Title = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 4:
                                instance.Series = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 5:
                                instance.Season = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 6:
                                instance.Url = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 7:
                                instance.Categories = await serializationStreamReader.ReadStringArrayAsync();
                                break;
                            case 8:
                                instance.VideoQuality = await serializationStreamReader.ReadAsAsync<ProductionQuality>();
                                break;
                            case 9:
                                instance.Keywords = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 10:
                                instance.ContentRating = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 11:
                                instance.UserRating = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 12:
                                instance.Context22 = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 13:
                                instance.IsLive = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 14:
                                instance.IsDirect = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 15:
                                instance.Producer = await serializationStreamReader.ReadAsAsync<Producer>();
                                break;
                            case 16:
                                instance.Length = await serializationStreamReader.ReadIntAsync();
                                break;
                            case 17:
                                instance.MediaRating = await serializationStreamReader.ReadAsAsync<MediaRating>();
                                break;
                            case 18:
                                instance.IsEmbeddable = await serializationStreamReader.ReadBooleanAsync();
                                break;
                            case 19:
                                instance.Language = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 20:
                                instance.Context = await serializationStreamReader.ReadAsAsync<Context>();
                                break;
                            case 21:
                                instance.Artist = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 22:
                                instance.Genre = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 23:
                                instance.Album = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 24:
                                instance.ISOCode = await serializationStreamReader.ReadStringAsync();
                                break;
                            case 25:
                                instance.Quality = await serializationStreamReader.ReadAsAsync<ProductionQuality>();
                                break;
                            default:
                                await serializationStreamReader.SkipAsync();
                                break;
                        }
                        break;
                    case "id":
                        instance.Id = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "episode":
                        instance.Episode = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "title":
                        instance.Title = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "series":
                        instance.Series = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "season":
                        instance.Season = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "url":
                        instance.Url = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "cat":
                        instance.Categories = await serializationStreamReader.ReadStringArrayAsync();
                        break;
                    case "videoquality":
                        instance.VideoQuality = await serializationStreamReader.ReadAsAsync<ProductionQuality>();
                        break;
                    case "keywords":
                        instance.Keywords = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "contentrating":
                        instance.ContentRating = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "userrating":
                        instance.UserRating = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "context_22":
                        instance.Context22 = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "livestream":
                        instance.IsLive = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "sourcerelationship":
                        instance.IsDirect = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "producer":
                        instance.Producer = await serializationStreamReader.ReadAsAsync<Producer>();
                        break;
                    case "len":
                        instance.Length = await serializationStreamReader.ReadIntAsync();
                        break;
                    case "qagmediarating":
                        instance.MediaRating = await serializationStreamReader.ReadAsAsync<MediaRating>();
                        break;
                    case "embeddable":
                        instance.IsEmbeddable = await serializationStreamReader.ReadBooleanAsync();
                        break;
                    case "language":
                        instance.Language = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "context":
                        instance.Context = await serializationStreamReader.ReadAsAsync<Context>();
                        break;
                    case "artist":
                        instance.Artist = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "genre":
                        instance.Genre = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "album":
                        instance.Album = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "isrc":
                        instance.ISOCode = await serializationStreamReader.ReadStringAsync();
                        break;
                    case "prodq":
                        instance.Quality = await serializationStreamReader.ReadAsAsync<ProductionQuality>();
                        break;
                    default:
                        await serializationStreamReader.SkipAsync();
                        break;

                }
            }
            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, Content instance)
            => WriteAsync(serializationStreamWriter, instance, CancellationToken.None).Wait();

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, Content instance, CancellationToken token)
        {
            if (instance != null)
            {
                await serializationStreamWriter.StartObjectAsync();
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 1, Name = "id" }, instance.Id);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 2, Name = "episode" }, instance.Episode);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 3, Name = "title" }, instance.Title);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 4, Name = "series" }, instance.Series);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 5, Name = "season" }, instance.Season);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 6, Name = "url" }, instance.Url);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 7, Name = "cat" }, instance.Categories);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 8, Name = "videoquality" }, instance.VideoQuality);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 9, Name = "keywords" }, instance.Keywords);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 10, Name = "contentrating" }, instance.ContentRating);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 11, Name = "userrating" }, instance.UserRating);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 12, Name = "context_22" }, instance.Context22);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 13, Name = "livestream" }, instance.IsLive);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 14, Name = "sourcerelationship" }, instance.IsDirect);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 15, Name = "producer" }, instance.Producer);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 16, Name = "len" }, instance.Length);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 17, Name = "qagmediarating" }, instance.MediaRating);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 18, Name = "embeddable" }, instance.IsEmbeddable);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 19, Name = "language" }, instance.Language);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 20, Name = "context" }, instance.Context);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 21, Name = "artist" }, instance.Artist);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 22, Name = "genre" }, instance.Genre);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 23, Name = "album" }, instance.Album);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 24, Name = "isrc" }, instance.ISOCode);
                await serializationStreamWriter.WriteAsync(new PropertyId { Id = 25, Name = "prodq" }, instance.Quality);
                await serializationStreamWriter.EndObjectAsync();
                await serializationStreamWriter.FlushAsync();
            }
        }
    }
}