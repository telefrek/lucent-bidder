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
        ISerializationContext _serializationContext;
        bool _leaveOpen;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="source">The source stream</param>
        /// <param name="streamFormat"></param>
        /// <param name="serializationContext"></param>
        /// <param name="leaveOpen"></param>
        public SerializationStream(Stream source, SerializationFormat streamFormat, ISerializationContext serializationContext, bool leaveOpen = false)
        {
            _wrappedStream = new BufferedStream(source, 4096);
            _streamFormat = streamFormat;
            _serializationContext = serializationContext;
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
            get => _serializationContext.CreateReader(_wrappedStream, _leaveOpen, _streamFormat);
        }

        /// <summary>
        /// Gets the appropriate writer for the serialization stream
        /// </summary>
        /// <returns>An initialized ISerializationStreamWriter</returns>
        public ISerializationStreamWriter Writer
        {
            get => _serializationContext.CreateWriter(_wrappedStream, _leaveOpen, _streamFormat);
        }
    }
}