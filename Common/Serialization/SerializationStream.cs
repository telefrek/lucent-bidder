using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Lucent.Common.Protobuf;
using Lucent.Common.Serialization.Json;
using Lucent.Common.Serialization.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Simple implementation of the ISerializationStream interface
    /// </summary>
    public class SerializationStream : ISerializationStream
    {
        Stream _wrappedStream;
        SerializationFormat _streamFormat;
        IServiceProvider _serviceProvider;
        bool _leaveOpen;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="source">The source stream</param>
        /// <param name="streamFormat"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="leaveOpen"></param>
        public SerializationStream(Stream source, SerializationFormat streamFormat, IServiceProvider serviceProvider, bool leaveOpen = false)
        {
            _wrappedStream = source;
            _streamFormat = streamFormat;
            _serviceProvider = serviceProvider;
            _leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Gets the stream serialization format
        /// </summary>
        public SerializationFormat Format => _streamFormat;

        /// <summary>
        /// Gets an appropriate reader for the stream
        /// </summary>
        /// <returns>An initialized ISerializationStreamReader</returns>
        public ISerializationStreamReader Reader
        {
            get
            {
                var target = _wrappedStream;
                if ((_streamFormat & SerializationFormat.COMPRESSED) == SerializationFormat.COMPRESSED)
                    target = new GZipStream(target, CompressionMode.Decompress, _leaveOpen);

                if ((_streamFormat & SerializationFormat.JSON) == SerializationFormat.JSON)
                {
                    var jsonReader = new JsonTextReader(new StreamReader(target, Encoding.UTF8, true, 4096, _leaveOpen));
                    if (_leaveOpen)
                        jsonReader.CloseInput = false;

                    return _serviceProvider.CreateInstance<JsonSerializationStreamReader>(jsonReader);
                }
                else if ((_streamFormat & SerializationFormat.PROTOBUF) == SerializationFormat.PROTOBUF)
                {
                    var protoReader = new ProtobufReader(target, _leaveOpen);
                    return _serviceProvider.CreateInstance<ProtobufSerializationStreamReader>(protoReader);
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the appropriate writer for the serialization stream
        /// </summary>
        /// <returns>An initialized ISerializationStreamWriter</returns>
        public ISerializationStreamWriter Writer
        {
            get
            {
                var target = _wrappedStream;
                if ((_streamFormat & SerializationFormat.COMPRESSED) == SerializationFormat.COMPRESSED)
                    target = new GZipStream(target, CompressionMode.Compress, _leaveOpen);

                if ((_streamFormat & SerializationFormat.JSON) == SerializationFormat.JSON)
                {
                    var jsonWriter = new JsonTextWriter(new StreamWriter(target, Encoding.UTF8, 4096, _leaveOpen));
                    return _serviceProvider.CreateInstance<JsonSerializationStreamWriter>(jsonWriter);

                }
                else if ((_streamFormat & SerializationFormat.PROTOBUF) == SerializationFormat.PROTOBUF)
                {
                    var protoWriter = new ProtobufWriter(target, _leaveOpen);
                    return _serviceProvider.CreateInstance<ProtobufSerializationStreamWriter>(protoWriter);
                }

                return null;
            }
        }
    }
}