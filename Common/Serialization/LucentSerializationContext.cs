using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        readonly ISerializationRegistry _registry;
        readonly ILogger _log;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="registry">The serialization registry to use</param>
        /// <param name="logger">A logger for the environment</param>
        public LucentSerializationContext(ISerializationRegistry registry, ILogger<LucentSerializationContext> logger)
        {
            _registry = registry;
            _log = logger;
        }

        /// <inheritdoc />
        public ISerializationStreamReader CreateReader(Stream target, bool leaveOpen, SerializationFormat format)
        {
            if (format.HasFlag(SerializationFormat.COMPRESSED))
                target = new GZipStream(target, CompressionMode.Compress, leaveOpen);

            if (format.HasFlag(SerializationFormat.PROTOBUF))
                return new ProtobufSerializationStreamReader(new ProtobufReader(target, leaveOpen), _registry, _log);
            else
                return new JsonSerializationStreamReader(new JsonTextReader(new StreamReader(target)) { CloseInput = !leaveOpen }, _registry, _log);
        }

        /// <inheritdoc />
        public ISerializationStreamWriter CreateWriter(Stream target, bool leaveOpen, SerializationFormat format)
        {
            if (format.HasFlag(SerializationFormat.COMPRESSED))
                target = new GZipStream(target, CompressionMode.Decompress, leaveOpen);

            if (format.HasFlag(SerializationFormat.PROTOBUF))
                return new ProtobufSerializationStreamWriter(new ProtobufWriter(target, leaveOpen), _registry, _log);
            else
                return new JsonSerializationStreamWriter(new JsonTextWriter(new StreamWriter(target, Encoding.UTF8, 4096, leaveOpen)), _registry, _log);
        }

        /// <inheritdoc />
        public ISerializationStream WrapStream(Stream target, bool leaveOpen, SerializationFormat format)
            => new SerializationStream(target, format, this, leaveOpen);


        /// <inheritdoc />
        public Task Write<T>(ILucentObjectWriter writer, T instance) where T : new()
        {
            var asm = AsyncTaskMethodBuilder.Create();
            var rsm = new ObjectWriterStateMachine<T>
            {
                AsyncBuilder = asm,
                Instance = instance,
                State = 1,
                Writer = writer,
                AwaiterMap = BuildMap<T>()
            };

            asm.Start(ref rsm);
            return rsm.AsyncBuilder.Task;
        }
                
        /// <inheritdoc />
        // public Task Write<T>(ILucentArrayWriter writer, T[] instance) where T : new()
        // {
        //     var asm = AsyncTaskMethodBuilder.Create();
        //     var rsm = new ObjectWriterStateMachine<T>
        //     {
        //         AsyncBuilder = asm,
        //         Instance = instance,
        //         State = 1,
        //         Writer = writer,
        //         AwaiterMap = BuildMap<T>()
        //     };

        //     asm.Start(ref rsm);
        //     return rsm.AsyncBuilder.Task;
        // }

        Func<T, ILucentObjectWriter, ulong, TaskAwaiter> BuildMap<T>() where T : new()
        {

            var ret = Expression.Label(typeof(TaskAwaiter));

            var list = (from p in typeof(T).GetProperties()
                        let attr = p.GetCustomAttributes(typeof(SerializationPropertyAttribute), true)
                        where attr.Length == 1
                        orderby (attr[0] as SerializationPropertyAttribute).Id
                        select new SerializableProperty { Property = p, Attribute = attr.Single() as SerializationPropertyAttribute }).ToArray();

            var cases = new List<SwitchCase>();

            var awaiter = typeof(Task).GetMethod("GetAwaiter");
            var retVar = Expression.Parameter(typeof(TaskAwaiter), "a");

            var mTypes = (from m in typeof(ILucentWriter).GetMethods()
                          let ptype = m.GetParameters().LastOrDefault()
                          where m.Name == "WriteAsync"
                          select new { PType = ptype, Method = m }).ToDictionary(e => e.PType.ParameterType, e => e.Method);

            var objParam = Expression.Parameter(typeof(T), "obj");
            var writerParam = Expression.Parameter(typeof(ILucentObjectWriter), "writer");
            var idParam = Expression.Parameter(typeof(ulong), "idx");

            foreach (var prop in list)
            {
                var convert = prop.Property.PropertyType.IsEnum;

                // Find the target method
                var mInfo = mTypes.GetValueOrDefault(prop.Property.PropertyType, null);
                if (mInfo == null && prop.Property.PropertyType.IsEnum)
                    mInfo = mTypes.GetValueOrDefault(typeof(int), null);

                // need to handle array/sub object here

                // Validate we found the method
                if (mInfo != null)
                {
                    // case idx:
                    //      return writer.WriteAsync(prop, val).GetAwaiter();
                    cases.Add(
                        Expression.SwitchCase(
                            Expression.Assign(retVar,
                                Expression.Call(
                                    Expression.Call(writerParam, mInfo, Expression.Constant(new PropertyId { Id = prop.Attribute.Id, Name = prop.Attribute.Name }),
                                        // Check for cast on enum
                                        convert ?
                                        (Expression)Expression.Convert(Expression.Property(objParam, prop.Property), typeof(int))
                                        : Expression.Property(objParam, prop.Property)
                                    ),
                                awaiter)
                            ),
                        Expression.Constant(prop.Attribute.Id)));
                }
            }

            // switch(idx) { 0-N in order, followed by default: return default(TaskAwaiter)}
            var body = new List<Expression>();
            body.Add(Expression.Switch(idParam, Expression.Assign(retVar, Expression.Default(typeof(TaskAwaiter))), cases.ToArray()));

            body.Add(Expression.Label(ret, retVar));

            var block = Expression.Block(new[] { retVar }, body);
            var ftype = typeof(Func<,,,>).MakeGenericType(typeof(T), typeof(ILucentObjectWriter), typeof(ulong), typeof(TaskAwaiter));

            var compiler = makeLambda.MakeGenericMethod(ftype).Invoke(null, new object[] { block, new ParameterExpression[] { objParam, writerParam, idParam } });

            return (Func<T, ILucentObjectWriter, ulong, TaskAwaiter>)compiler.GetType().GetMethod("Compile", Type.EmptyTypes).Invoke(compiler, new object[] { });
        }

        static readonly MethodInfo makeLambda = typeof(Expression).GetMethods().Where(m =>
                m.Name == "Lambda" && m.IsGenericMethod && m.GetGenericArguments().Length == 1
                ).First();
    }
}