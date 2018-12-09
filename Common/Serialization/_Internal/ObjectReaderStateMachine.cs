using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lucent.Common.Serialization._Internal
{
    /// <summary>
    /// Implementation of an async state machine for objects
    /// </summary>
    /// <remarks>
    /// Need this to create asynchronous readers/writers via Expressions + Lambdas
    /// </remarks>
    [CompilerGenerated]
    [StructLayout(LayoutKind.Auto)]
    struct ObjectReaderStateMachine<T> : IAsyncStateMachine
    {
        public ILucentObjectReader Reader;
        public int State;
        public AsyncTaskMethodBuilder<T> AsyncBuilder;
        public T Instance;
        public bool Finished;
        public ISerializationContext Context;
        TaskAwaiter<bool> completeAwaiter;
        TaskAwaiter<PropertyId> propertyAwaiter;

        PropertyId property;
        public Func<T, ILucentObjectReader, ISerializationContext, PropertyId, TaskAwaiter> AwaiterMap;

        public void MoveNext()
        {
            try
            {
                while (!Finished)
                {
                    if (State == 0)
                    {
                        completeAwaiter = Reader.IsComplete().GetAwaiter();
                        State = 1;

                        if (!completeAwaiter.IsCompleted)
                        {
                            AsyncBuilder.AwaitUnsafeOnCompleted(ref completeAwaiter, ref this);
                            return;
                        }
                    }

                    if (State == 1)
                    {
                        Finished = completeAwaiter.GetResult();

                        if (!Finished)
                        {
                            propertyAwaiter = Reader.NextAsync().GetAwaiter();
                            State = 2;

                            if (!propertyAwaiter.IsCompleted)
                            {
                                AsyncBuilder.AwaitUnsafeOnCompleted(ref propertyAwaiter, ref this);
                                return;
                            }
                        }
                        else
                            break;
                    }

                    if (State == 2)
                    {
                        property = propertyAwaiter.GetResult();
                        if(property == null)
                            break;

                        var aw = AwaiterMap(Instance, Reader, Context, property);
                        State = 0;

                        if (!aw.IsCompleted)
                        {
                            AsyncBuilder.AwaitUnsafeOnCompleted(ref aw, ref this);
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                State = -2;
                AsyncBuilder.SetException(e);
                return;
            }

            State = -1;
            AsyncBuilder.SetResult(Instance);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine) => AsyncBuilder.SetStateMachine(stateMachine);
    }
}