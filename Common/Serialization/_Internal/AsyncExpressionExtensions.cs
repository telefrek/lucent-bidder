using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Lucent.Common.Serialization._Internal
{
    /// <summary>
    /// Helper class for building asynchronous expressions
    /// </summary>
    public static class AsyncExpressionExtensions
    {
        /// <summary>
        /// Write the instance to the writer asynchronously
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="context"></param>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task Write<T>(this ILucentObjectWriter writer, ISerializationContext context, T instance) where T : new()
        {
            var asm = AsyncTaskMethodBuilder.Create();
            var rsm = new ObjectWriterStateMachine<T>
            {
                AsyncBuilder = asm,
                Instance = instance,
                State = 1,
                Writer = writer,
                AwaiterMap = BuildWriter<T>(),
                Context = context,
            };

            asm.Start(ref rsm);
            return rsm.AsyncBuilder.Task;
        }

        /// <summary>
        /// Write the instances to the writer asynchronously
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="context"></param>
        /// <param name="instances"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task Write<T>(this ILucentArrayWriter writer, ISerializationContext context, T[] instances)
        {
            var asm = AsyncTaskMethodBuilder.Create();
            var rsm = new ArrayWriterStateMachine<T>
            {
                AsyncBuilder = asm,
                Instances = instances,
                State = 0,
                Writer = writer,
                WriteObj = BuildArrayWriter<T>(),
                Context = context,
            };

            asm.Start(ref rsm);
            return rsm.AsyncBuilder.Task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="context"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<T> Read<T>(this ILucentObjectReader reader, ISerializationContext context) where T : new()
        {
            var asm = AsyncTaskMethodBuilder<T>.Create();
            var rsm = new ObjectReaderStateMachine<T>
            {
                AsyncBuilder = asm,
                Instance = new T(),
                State = 0,
                Reader = reader,
                AwaiterMap = BuildReader<T>(),
                Context = context,
            };

            asm.Start(ref rsm);
            return rsm.AsyncBuilder.Task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="context"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<T[]> ReadArray<T>(this ILucentArrayReader reader, ISerializationContext context)
        {
            var asm = AsyncTaskMethodBuilder<T[]>.Create();
            var rsm = new ArrayReaderStateMachine<T>
            {
                AsyncBuilder = asm,
                Instances = new List<T>(),
                State = 0,
                Reader = reader,
                AwaiterMap = BuildArrayReader<T>(),
                Context = context,
            };

            asm.Start(ref rsm);
            return rsm.AsyncBuilder.Task;
        }

        static Func<T, ILucentArrayWriter, ISerializationContext, TaskAwaiter> BuildArrayWriter<T>()
        {
            var ret = Expression.Label(typeof(TaskAwaiter));

            var mTypes = (from m in typeof(ILucentArrayWriter).GetMethods()
                          let ptype = m.GetParameters().LastOrDefault()
                          where m.Name == "WriteAsync"
                          select new { PType = ptype, Method = m }).ToDictionary(e => e.PType.ParameterType, e => e.Method);

            var aType = typeof(T);
            var convert = aType.IsEnum;

            var objParam = Expression.Parameter(typeof(T), "obj");
            var writerParam = Expression.Parameter(typeof(ILucentArrayWriter), "writer");
            var contextParam = Expression.Parameter(typeof(ISerializationContext), "context");

            // Find the target method
            var mInfo = mTypes.GetValueOrDefault(aType, null);

            var valExp = (Expression)objParam;

            var body = new List<Expression>();
            var awaiter = typeof(Task).GetMethod("GetAwaiter");

            // Handle casting objects
            if (mInfo == null && convert)
            {
                mInfo = mTypes.GetValueOrDefault(typeof(int), null);
                valExp = Expression.Convert(valExp, typeof(int));
                body.Add(Expression.Return(ret, Expression.Call(Expression.Call(writerParam, mInfo, objParam), awaiter)));
            }
            else if (mInfo != null)
            {
                body.Add(Expression.Return(ret, Expression.Call(Expression.Call(writerParam, mInfo, objParam), awaiter)));
            }
            else if (!aType.IsValueType && aType.GetConstructor(Type.EmptyTypes) != null)
            {
                mInfo = typeof(ISerializationContext).GetMethod("WriteArrayObject").MakeGenericMethod(aType);
                body.Add(Expression.Return(ret, Expression.Call(Expression.Call(contextParam, mInfo, writerParam, objParam), awaiter)));
            }

            body.Add(Expression.Label(ret, Expression.Default(typeof(TaskAwaiter))));

            var block = Expression.Block(body);
            var ftype = typeof(Func<,,,>).MakeGenericType(typeof(T), typeof(ILucentArrayWriter), typeof(ISerializationContext), typeof(TaskAwaiter));

            var compiler = makeLambda.MakeGenericMethod(ftype).Invoke(null, new object[] { block, new ParameterExpression[] { objParam, writerParam, contextParam } });

            return (Func<T, ILucentArrayWriter, ISerializationContext, TaskAwaiter>)compiler.GetType().GetMethod("Compile", Type.EmptyTypes).Invoke(compiler, new object[] { });
        }

        static Func<T, ILucentObjectWriter, ISerializationContext, ulong, TaskAwaiter> BuildWriter<T>() where T : new()
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
                else if (prop.Property.PropertyType.IsArray)
                {
                    var writeMeth = typeof(ISerializationContext).GetMethod("WriteArray").MakeGenericMethod(prop.Property.PropertyType.GetElementType());
                    cases.Add(
                        Expression.SwitchCase(
                            Expression.Return(ret,
                                Expression.Call(
                                    Expression.Call(contextParam, writeMeth, writerParam, Expression.Constant(new PropertyId { Id = prop.Attribute.Id, Name = prop.Attribute.Name }),
                                    propExp),
                                awaiter)
                            ),
                        Expression.Constant(prop.Attribute.Id)));
                }
                // Object
                else if (!prop.Property.PropertyType.IsValueType && prop.Property.PropertyType.GetConstructor(Type.EmptyTypes) != null)
                {
                    var writeMeth = typeof(ISerializationContext).GetMethod("WriteObject").MakeGenericMethod(prop.Property.PropertyType);
                    cases.Add(
                        Expression.SwitchCase(
                            Expression.Return(ret,
                                Expression.Call(
                                    Expression.Call(contextParam, writeMeth, writerParam, Expression.Constant(new PropertyId { Id = prop.Attribute.Id, Name = prop.Attribute.Name }),
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


        static Func<ILucentArrayReader, ISerializationContext, TaskAwaiter<T>> BuildArrayReader<T>()
        {
            var ret = Expression.Label(typeof(TaskAwaiter<T>));

            var mTypes = (from m in typeof(ILucentArrayWriter).GetMethods()
                          let ptype = m.ReturnType.GetGenericArguments()
                          where m.Name.StartsWith("Next") && !m.Name.EndsWith("Async") && m.ReturnType.IsGenericType
                          select new { PType = ptype, Method = m }).ToDictionary(e => e.PType.Single(), e => e.Method);

            var aType = typeof(T);
            var convert = aType.IsEnum;

            var objParam = Expression.Parameter(typeof(T), "obj");
            var readerParam = Expression.Parameter(typeof(ILucentArrayReader), "reader");
            var contextParam = Expression.Parameter(typeof(ISerializationContext), "context");

            // Find the target method
            var mInfo = mTypes.GetValueOrDefault(aType, null);

            var valExp = (Expression)objParam;

            var body = new List<Expression>();
            var awaiter = typeof(Task).GetMethod("GetAwaiter");
            var continuation = typeof(Task<>).GetMethods().First(m => m.Name == "ContinueWith" && m.GetParameters().Length == 2 &&
            m.GetParameters()[1].ParameterType == typeof(object));


            var skip = typeof(ILucentReader).GetMethod("Skip");

            body.Add(Expression.Label(ret, Expression.Default(typeof(TaskAwaiter<T>))));

            var block = Expression.Block(body);
            var ftype = typeof(Func<,,>).MakeGenericType(typeof(ILucentArrayReader), typeof(ISerializationContext), typeof(TaskAwaiter<>).MakeGenericType(typeof(T)));

            var compiler = makeLambda.MakeGenericMethod(ftype).Invoke(null, new object[] { block, new ParameterExpression[] { objParam, readerParam, contextParam } });

            return (Func<ILucentArrayReader, ISerializationContext, TaskAwaiter<T>>)compiler.GetType().GetMethod("Compile", Type.EmptyTypes).Invoke(compiler, new object[] { });
        }

        static Func<T, ILucentObjectReader, ISerializationContext, PropertyId, TaskAwaiter> BuildReader<T>() where T : new()
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

                if (!sType.IsValueType && sType.GetConstructor(Type.EmptyTypes) != null)
                {
                    // Type has to be object and new()
                    var readMethod = typeof(ISerializationContext).GetMethods().First(m => m.Name == "ReadObject" && m.IsGenericMethod).MakeGenericMethod(sType);

                    // Have to get an object reader from the current one
                    var tParam = Expression.Parameter(typeof(Task<>).MakeGenericType(sType), "t");
                    var cObjParam = Expression.Parameter(typeof(object), "o");
                    var tRes = Expression.Property(tParam, "Result");

                    var read = Expression.Call(contextParam, readMethod, readerParam);
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
                else if (sType.IsArray)
                {
                    // Type has to be object and new()
                    var readMethod = typeof(ISerializationContext).GetMethods().First(m => m.Name == "ReadArray" && m.IsGenericMethod).MakeGenericMethod(sType.GetElementType());

                    // Have to get an object reader from the current one
                    var tParam = Expression.Parameter(typeof(Task<>).MakeGenericType(sType), "t");
                    var cObjParam = Expression.Parameter(typeof(object), "o");
                    var tRes = Expression.Property(tParam, "Result");

                    var read = Expression.Call(contextParam, readMethod, readerParam);
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
                else
                {
                    var mInfo = mTypes.GetValueOrDefault(sType, null);

                    if (sType.IsEnum)
                    {
                        mInfo = typeof(ILucentReader).GetMethod("NextEnum").MakeGenericMethod(sType);
                    }

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