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
        public Task<T> Read<T>(ILucentObjectReader reader) where T : new()
        {
            var asm = AsyncTaskMethodBuilder<T>.Create();
            var rsm = new ObjectReaderStateMachine<T>
            {
                AsyncBuilder = asm,
                Instance = new T(),
                State = 0,
                Reader = reader,
                AwaiterMap = BuildReader<T>()
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

        Func<T, ILucentObjectWriter, ISerializationContext, ulong, TaskAwaiter> BuildMap<T>() where T : new()
        {

            var ret = Expression.Label(typeof(TaskAwaiter));

            var list = (from p in typeof(T).GetProperties()
                        let attr = p.GetCustomAttributes(typeof(SerializationPropertyAttribute), true)
                        where attr.Length == 1
                        orderby (attr[0] as SerializationPropertyAttribute).Id
                        select new SerializableProperty { Property = p, Attribute = attr.Single() as SerializationPropertyAttribute }).ToArray();

            var cases = new List<SwitchCase>();

            var awaiter = typeof(Task).GetMethod("GetAwaiter");

            var mTypes = (from m in typeof(ILucentWriter).GetMethods()
                          let ptype = m.GetParameters().LastOrDefault()
                          where m.Name == "WriteAsync"
                          select new { PType = ptype, Method = m }).ToDictionary(e => e.PType.ParameterType, e => e.Method);

            var objParam = Expression.Parameter(typeof(T), "obj");
            var writerParam = Expression.Parameter(typeof(ILucentObjectWriter), "writer");
            var contextParam = Expression.Parameter(typeof(ISerializationContext), "context");
            var idParam = Expression.Parameter(typeof(ulong), "idx");

            foreach (var prop in list)
            {
                var convert = prop.Property.PropertyType.IsEnum;

                // Find the target method
                var mInfo = mTypes.GetValueOrDefault(prop.Property.PropertyType, null);
                Expression propExp = Expression.Property(objParam, prop.Property);

                // Handle casted objects
                if (mInfo == null && prop.Property.PropertyType.IsEnum)
                {
                    mInfo = mTypes.GetValueOrDefault(typeof(int), null);
                    propExp = Expression.Convert(propExp, typeof(int));
                }
                else if (mInfo == null && prop.Property.PropertyType == typeof(Guid))
                {
                    mInfo = mTypes.GetValueOrDefault(typeof(string), null);
                    propExp = Expression.Call(propExp, typeof(Guid).GetMethods().First(m => m.Name == "ToString"));
                }
                else if (mInfo == null && prop.Property.PropertyType == typeof(DateTime))
                {
                    mInfo = mTypes.GetValueOrDefault(typeof(long), null);
                    propExp = Expression.Call(propExp, typeof(DateTime).GetMethods().First(m => m.Name == "ToFileTimeUtc"));
                }
                // need to handle array/sub object here

                // Validate we found the method
                if (mInfo != null)
                {
                    // case idx:
                    //      return writer.WriteAsync(prop, val).GetAwaiter();
                    cases.Add(
                        Expression.SwitchCase(
                            Expression.Return(ret,
                                Expression.Call(
                                    Expression.Call(writerParam, mInfo, Expression.Constant(new PropertyId { Id = prop.Attribute.Id, Name = prop.Attribute.Name }),
                                    propExp),
                                awaiter)
                            ),
                        Expression.Constant(prop.Attribute.Id)));
                }
            }

            // switch(idx) { 0-N in order, followed by default: return default(TaskAwaiter)}
            var body = new List<Expression>();
            body.Add(Expression.Switch(idParam, Expression.Return(ret, Expression.Default(typeof(TaskAwaiter))), cases.ToArray()));

            body.Add(Expression.Label(ret, Expression.Default(typeof(TaskAwaiter))));

            var block = Expression.Block(body);
            var ftype = typeof(Func<,,,,>).MakeGenericType(typeof(T), typeof(ILucentObjectWriter), typeof(ISerializationContext), typeof(ulong), typeof(TaskAwaiter));

            var compiler = makeLambda.MakeGenericMethod(ftype).Invoke(null, new object[] { block, new ParameterExpression[] { objParam, writerParam, contextParam, idParam } });

            return (Func<T, ILucentObjectWriter, ISerializationContext, ulong, TaskAwaiter>)compiler.GetType().GetMethod("Compile", Type.EmptyTypes).Invoke(compiler, new object[] { });
        }

        Func<T, ILucentObjectReader, ISerializationContext, PropertyId, TaskAwaiter> BuildReader<T>() where T : new()
        {
            var ret = Expression.Label(typeof(TaskAwaiter));

            var list = (from p in typeof(T).GetProperties()
                        let attr = p.GetCustomAttributes(typeof(SerializationPropertyAttribute), true)
                        where attr.Length == 1
                        orderby (attr[0] as SerializationPropertyAttribute).Id
                        select new SerializableProperty { Property = p, Attribute = attr.Single() as SerializationPropertyAttribute }).ToArray();

            var body = new List<Expression>();

            var awaiter = typeof(Task).GetMethod("GetAwaiter");
            var continuation = typeof(Task<>).GetMethods().First(m => m.Name == "ContinueWith" && m.GetParameters().Length == 2 &&
            m.GetParameters()[1].ParameterType == typeof(object));

            var mTypes = (from m in typeof(ILucentReader).GetMethods()
                          let ptype = m.ReturnType.GetGenericArguments()
                          where m.Name.StartsWith("Next") && !m.Name.EndsWith("Async") && m.ReturnType.IsGenericType
                          select new { PType = ptype, Method = m }).ToDictionary(e => e.PType.Single(), e => e.Method);

            var skip = typeof(ILucentReader).GetMethod("Skip");

            var objParam = Expression.Parameter(typeof(T), "obj");
            var readerParam = Expression.Parameter(typeof(ILucentObjectReader), "reader");
            var contextParam = Expression.Parameter(typeof(ISerializationContext), "context");
            var idParam = Expression.Parameter(typeof(PropertyId), "id");

            foreach (var prop in list)
            {
                // Create a bunch of if statements lol
                var sType = prop.Property.PropertyType;
                Expression propExp = Expression.Property(objParam, prop.Property);

                if (sType.IsEnum)
                {
                    sType = typeof(int);
                    var mInfo = mTypes.GetValueOrDefault(sType, null);
                    if (mInfo != null)
                    {
                        var eMethod = typeof(Enum).GetMethods(BindingFlags.Static | BindingFlags.Public).First(m => m.Name == "ToObject" && m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType == typeof(int));

                        var tParam = Expression.Parameter(typeof(Task<>).MakeGenericType(sType), "t");
                        var cObjParam = Expression.Parameter(typeof(object), "o");
                        var tRes = Expression.Convert(Expression.Call(eMethod, Expression.Constant(prop.Property.PropertyType), Expression.Property(tParam, "Result")), prop.Property.PropertyType);

                        var read = Expression.Call(readerParam, mInfo);
                        var lambda = (Expression)makeLambda.MakeGenericMethod(typeof(Action<,>).MakeGenericType(typeof(Task<>).MakeGenericType(sType), typeof(object))).Invoke(null, new object[] { Expression.Block(new[] { Expression.Assign(Expression.Property(Expression.Convert(cObjParam, typeof(T)), prop.Property), tRes) }), new[] { tParam, cObjParam } });

                        var tcont = typeof(Task<>).MakeGenericType(sType).GetMethods().First(m => m.Name == "ContinueWith" && m.GetParameters().Length == 2 &&
                        m.GetParameters()[1].ParameterType == typeof(object));

                        var cont = Expression.Call(
                                            read,
                                            tcont,

                                            // Hook the Task.ContinueWith((t,o)=>((T)o).Prop = t.Result)
                                            lambda,
                                            objParam
                                        );

                        body.Add(Expression.IfThen(
                            // Test the property value
                            Expression.OrElse(Expression.Equal(Expression.Property(idParam, "Id"), Expression.Constant(prop.Attribute.Id)), Expression.Equal(Expression.Property(idParam, "Name"), Expression.Constant(prop.Attribute.Name))),
                                // Return the awaiter
                                Expression.Return(ret,
                                    // Get the awaiter
                                    Expression.Call(
                                        // Call the reader method
                                        cont,
                                        awaiter
                                    )
                                )
                            )
                        );
                    }
                }
                else if (sType == typeof(Guid))
                {
                    sType = typeof(string);
                    var mInfo = mTypes.GetValueOrDefault(sType, null);
                    if (mInfo != null)
                    {
                        var eMethod = typeof(Guid).GetMethods(BindingFlags.Static | BindingFlags.Public).First(m => m.Name == "Parse" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));

                        var tParam = Expression.Parameter(typeof(Task<>).MakeGenericType(sType), "t");
                        var cObjParam = Expression.Parameter(typeof(object), "o");
                        var tRes = Expression.Call(eMethod, Expression.Constant(prop.Property.PropertyType), Expression.Property(tParam, "Result"));

                        var read = Expression.Call(readerParam, mInfo);
                        var lambda = (Expression)makeLambda.MakeGenericMethod(typeof(Action<,>).MakeGenericType(typeof(Task<>).MakeGenericType(sType), typeof(object))).Invoke(null, new object[] { Expression.Block(new[] { Expression.Assign(Expression.Property(Expression.Convert(cObjParam, typeof(T)), prop.Property), tRes) }), new[] { tParam, cObjParam } });

                        var tcont = typeof(Task<>).MakeGenericType(sType).GetMethods().First(m => m.Name == "ContinueWith" && m.GetParameters().Length == 2 &&
                        m.GetParameters()[1].ParameterType == typeof(object));

                        var cont = Expression.Call(
                                            read,
                                            tcont,

                                            // Hook the Task.ContinueWith((t,o)=>((T)o).Prop = t.Result)
                                            lambda,
                                            objParam
                                        );

                        body.Add(Expression.IfThen(
                            // Test the property value
                            Expression.OrElse(Expression.Equal(Expression.Property(idParam, "Id"), Expression.Constant(prop.Attribute.Id)), Expression.Equal(Expression.Property(idParam, "Name"), Expression.Constant(prop.Attribute.Name))),
                                // Return the awaiter
                                Expression.Return(ret,
                                    // Get the awaiter
                                    Expression.Call(
                                        // Call the reader method
                                        cont,
                                        awaiter
                                    )
                                )
                            )
                        );
                    }
                }
                else if (sType == typeof(DateTime))
                {
                    sType = typeof(long);
                    var mInfo = mTypes.GetValueOrDefault(sType, null);
                    if (mInfo != null)
                    {
                        var eMethod = typeof(DateTime).GetMethods(BindingFlags.Static | BindingFlags.Public).First(m => m.Name == "FromFileTimeUtc" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(long));

                        var tParam = Expression.Parameter(typeof(Task<>).MakeGenericType(sType), "t");
                        var cObjParam = Expression.Parameter(typeof(object), "o");
                        var tRes = Expression.Call(eMethod, Expression.Constant(prop.Property.PropertyType), Expression.Property(tParam, "Result"));

                        var read = Expression.Call(readerParam, mInfo);
                        var lambda = (Expression)makeLambda.MakeGenericMethod(typeof(Action<,>).MakeGenericType(typeof(Task<>).MakeGenericType(sType), typeof(object))).Invoke(null, new object[] { Expression.Block(new[] { Expression.Assign(Expression.Property(Expression.Convert(cObjParam, typeof(T)), prop.Property), tRes) }), new[] { tParam, cObjParam } });

                        var tcont = typeof(Task<>).MakeGenericType(sType).GetMethods().First(m => m.Name == "ContinueWith" && m.GetParameters().Length == 2 &&
                        m.GetParameters()[1].ParameterType == typeof(object));

                        var cont = Expression.Call(
                                            read,
                                            tcont,

                                            // Hook the Task.ContinueWith((t,o)=>((T)o).Prop = t.Result)
                                            lambda,
                                            objParam
                                        );

                        body.Add(Expression.IfThen(
                            // Test the property value
                            Expression.OrElse(Expression.Equal(Expression.Property(idParam, "Id"), Expression.Constant(prop.Attribute.Id)), Expression.Equal(Expression.Property(idParam, "Name"), Expression.Constant(prop.Attribute.Name))),
                                // Return the awaiter
                                Expression.Return(ret,
                                    // Get the awaiter
                                    Expression.Call(
                                        // Call the reader method
                                        cont,
                                        awaiter
                                    )
                                )
                            )
                        );
                    }
                }
                // Handle object/array here
                else
                {
                    var mInfo = mTypes.GetValueOrDefault(sType, null);
                    if (mInfo != null)
                    {
                        var tParam = Expression.Parameter(typeof(Task<>).MakeGenericType(sType), "t");
                        var cObjParam = Expression.Parameter(typeof(object), "o");
                        var tRes = Expression.Property(tParam, "Result");

                        var read = Expression.Call(readerParam, mInfo);
                        var lambda = (Expression)makeLambda.MakeGenericMethod(typeof(Action<,>).MakeGenericType(typeof(Task<>).MakeGenericType(sType), typeof(object))).Invoke(null, new object[] { Expression.Block(new[] { Expression.Assign(Expression.Property(Expression.Convert(cObjParam, typeof(T)), prop.Property), tRes) }), new[] { tParam, cObjParam } });

                        var tcont = typeof(Task<>).MakeGenericType(sType).GetMethods().First(m => m.Name == "ContinueWith" && m.GetParameters().Length == 2 &&
                        m.GetParameters()[1].ParameterType == typeof(object));

                        var cont = Expression.Call(
                                            read,
                                            tcont,

                                            // Hook the Task.ContinueWith((t,o)=>((T)o).Prop = t.Result, obj)
                                            lambda,
                                            objParam
                                        );

                        body.Add(Expression.IfThen(
                            // Test the property value
                            Expression.OrElse(Expression.Equal(Expression.Property(idParam, "Id"), Expression.Constant(prop.Attribute.Id)), Expression.Equal(Expression.Property(idParam, "Name"), Expression.Constant(prop.Attribute.Name))),
                                // Return the awaiter
                                Expression.Return(ret,
                                    // Get the awaiter
                                    Expression.Call(
                                        // Call the reader method
                                        cont,
                                        awaiter
                                    )
                                )
                            )
                        );
                    }
                }
            }

            // Default is to skip the value
            body.Add(Expression.Label(ret, Expression.Call(Expression.Call(readerParam, skip), awaiter)));

            var block = Expression.Block(body);
            var ftype = typeof(Func<,,,,>).MakeGenericType(typeof(T), typeof(ILucentObjectReader), typeof(ISerializationContext), typeof(PropertyId), typeof(TaskAwaiter));

            var compiler = makeLambda.MakeGenericMethod(ftype).Invoke(null, new object[] { block, new ParameterExpression[] { objParam, readerParam, contextParam, idParam } });

            return (Func<T, ILucentObjectReader, ISerializationContext, PropertyId, TaskAwaiter>)compiler.GetType().GetMethod("Compile", Type.EmptyTypes).Invoke(compiler, new object[] { });
        }

        static readonly MethodInfo makeLambda = typeof(Expression).GetMethods().Where(m =>
                m.Name == "Lambda" && m.IsGenericMethod && m.GetGenericArguments().Length == 1
                ).First();
    }
}