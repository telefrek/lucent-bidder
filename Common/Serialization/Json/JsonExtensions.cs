using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Lucent.Common.Serialization.Json
{
    /// <summary>
    /// Extensions for Json format reader/writer/utilities
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Task<ILucentWriter> CreateJsonWriter(this TextWriter writer, JsonFormat format = default(JsonFormat)) => Task.FromResult<ILucentWriter>(new LucentJsonWriter(new JsonTextWriter(writer), format));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="leaveOpen"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static async Task<ILucentWriter> CreateJsonWriter(this Stream target, bool leaveOpen = false, JsonFormat format = default(JsonFormat)) => await new StreamWriter(target, new UTF8Encoding(false), 4096, leaveOpen).CreateJsonWriter(format);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static async Task<ILucentObjectWriter> CreateJsonObjectWriter(this TextWriter writer, JsonFormat format = default(JsonFormat))
        {
            var jsonWriter = new JsonTextWriter(writer);
            await jsonWriter.WriteStartObjectAsync();
            return new LucentJsonObjectWriter(jsonWriter, format);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="leaveOpen"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static async Task<ILucentObjectWriter> CreateJsonObjectWriter(this Stream target, bool leaveOpen = false, JsonFormat format = default(JsonFormat)) => await new StreamWriter(target, new UTF8Encoding(false), 4096, leaveOpen).CreateJsonObjectWriter(format);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static async Task<ILucentArrayWriter> CreateJsonArrayWriter(this TextWriter writer, JsonFormat format = default(JsonFormat))
        {
            var jsonWriter = new JsonTextWriter(writer);
            await jsonWriter.WriteStartArrayAsync();
            return new LucentJsonArrayWriter(jsonWriter, format);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="leaveOpen"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static async Task<ILucentArrayWriter> CreateJsonArrayWriter(this Stream target, bool leaveOpen = false, JsonFormat format = default(JsonFormat)) => await new StreamWriter(target, new UTF8Encoding(false), 4096, leaveOpen).CreateJsonArrayWriter(format);
    }
}