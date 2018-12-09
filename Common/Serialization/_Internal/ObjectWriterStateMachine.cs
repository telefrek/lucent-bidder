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
    struct ObjectWriterStateMachine<T> : IAsyncStateMachine
    {
        public ILucentObjectWriter Writer;
        public ISerializationContext Context;
        public int State;
        public AsyncTaskMethodBuilder AsyncBuilder;
        public T Instance;
        public bool Finished;
        public Func<T, ILucentObjectWriter, ISerializationContext, ulong, TaskAwaiter> AwaiterMap;

        public void MoveNext()
        {
            try
            {
                while (!Finished)
                {
                    var aw = AwaiterMap(Instance, Writer, Context, (ulong)State);
                    if (aw.Equals(default(TaskAwaiter)))
                    {
                        Finished = true;
                        aw = Writer.EndObject().GetAwaiter();
                    }

                    State++;

                    if (!aw.IsCompleted)
                    {
                        AsyncBuilder.AwaitUnsafeOnCompleted(ref aw, ref this);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                State = int.MaxValue;
                AsyncBuilder.SetException(e);
                return;
            }

            State = int.MaxValue;
            AsyncBuilder.SetResult();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine) => AsyncBuilder.SetStateMachine(stateMachine);
    }
}