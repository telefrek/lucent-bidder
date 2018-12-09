using System;
using System.Collections.Generic;
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
    struct ArrayReaderStateMachine<T> : IAsyncStateMachine
    {
        public ILucentArrayReader Reader;
        public int State;
        public AsyncTaskMethodBuilder<T[]> AsyncBuilder;
        public List<T> Instances;
        public bool Finished;
        public ISerializationContext Context;
        TaskAwaiter<bool> completeAwaiter;
        TaskAwaiter<T> valAwaiter;

        public Func<ILucentArrayReader, ISerializationContext, TaskAwaiter<T>> AwaiterMap;

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
                            valAwaiter = AwaiterMap(Reader, Context);
                            State = 2;

                            if (!valAwaiter.IsCompleted)
                            {
                                AsyncBuilder.AwaitUnsafeOnCompleted(ref valAwaiter, ref this);
                                return;
                            }
                        }
                        else
                            break;
                    }

                    if (State == 2)
                    {
                        var item = valAwaiter.GetResult();
                        if (!item.IsNullOrDefault())
                            Instances.Add(item);

                        State = 0;
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
            AsyncBuilder.SetResult(Instances.ToArray());
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine) => AsyncBuilder.SetStateMachine(stateMachine);
    }
}