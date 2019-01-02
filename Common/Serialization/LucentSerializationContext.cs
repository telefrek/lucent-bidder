using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common.Protobuf;
using Lucent.Common.Serialization._Internal;
using Lucent.Common.Serialization.Json;
using Lucent.Common.Serialization.Protobuf;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Default implementation for serialization contexts
    /// </summary>
    public class LucentSerializationContext : ISerializationContext
    {
        readonly ILogger _log;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">A logger for the environment</param>
        public LucentSerializationContext(ILogger<LucentSerializationContext> logger)
        {
            _log = logger;
        }


        /// <inheritdoc />
        public async Task Write<T>(ILucentObjectWriter writer, T instance) where T : new()
        {
            if (!instance.IsNullOrDefault())
                await writer.Write(this, instance);
            await writer.Flush();

        }

        /// <inheritdoc />
        public async Task WriteObject<T>(ILucentWriter writer, PropertyId property, T instance) where T : new()
        {
            if (!instance.IsNullOrDefault())
                await Write<T>(await writer.CreateObjectWriter(property), instance);
            await writer.Flush();
        }

        /// <inheritdoc />
        public async Task WriteArrayObject<T>(ILucentArrayWriter writer, T instance) where T : new()
        {
            if (!instance.IsNullOrDefault())
                await Write<T>(await writer.CreateObjectWriter(), instance);
            await writer.Flush();
        }

        /// <inheritdoc />
        public async Task WriteArray<T>(ILucentWriter writer, PropertyId property, T[] instances)
        {
            if (!instances.IsNullOrDefault())
                await (await writer.CreateArrayWriter(property)).Write(this, instances);
            await writer.Flush();
        }

        /// <inheritdoc />
        public async Task<T> Read<T>(ILucentObjectReader reader) where T : new()
        {
            using (reader)
                return await reader.Read<T>(this);
        }

        /// <inheritdoc />
        public async Task<T[]> ReadArray<T>(ILucentReader reader)
        {
            using (var arrReader = await reader.GetArrayReader())
                return await arrReader.ReadArray<T>(this);
        }

        /// <inheritdoc/>
        public async Task<T> ReadObject<T>(ILucentReader reader) where T : new() => await Read<T>(await reader.GetObjectReader());

        /// <inheritdoc/>
        public async Task<T> ReadArrayObject<T>(ILucentArrayReader reader) where T : new() => await Read<T>(await reader.GetObjectReader());

        /// <inheritdoc />
        public async Task<T> ReadFrom<T>(Stream target, bool leaveOpen, SerializationFormat format)
            where T : new()
        {
            if (format.HasFlag(SerializationFormat.COMPRESSED))
                target = new GZipStream(target, CompressionMode.Decompress);

            switch (format)
            {
                case SerializationFormat.PROTOBUF:
                    return await ReadObject<T>(new LucentProtoReader(target, leaveOpen));
                default:
                    return await ReadObject<T>(new LucentJsonReader(target, leaveOpen));
            }
        }

        /// <inheritdoc />
        public async Task WriteTo<T>(T instance, Stream target, bool leaveOpen, SerializationFormat format)
            where T : new()
        {
            if (format.HasFlag(SerializationFormat.COMPRESSED))
                target = new GZipStream(target, CompressionMode.Compress);

            switch (format)
            {
                case SerializationFormat.PROTOBUF:
                    using (var writer = await new LucentProtoWriter(target, leaveOpen).AsObjectWriter())
                        await Write<T>(writer, instance);
                    break;
                default:
                    using (var writer = await target.CreateJsonObjectWriter(leaveOpen))
                        await Write<T>(writer, instance);
                    break;
            }
        }

    }
}