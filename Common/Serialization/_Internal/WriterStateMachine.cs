using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lucent.Common.Serialization._Internal
{
    /// <summary>
    /// Implementation of an async state machine
    /// </summary>
    /// <remarks>
    /// Need this to create asynchronous readers via Expressions + Lambdas
    /// </remarks>
    [CompilerGenerated]
    [StructLayout(LayoutKind.Auto)]
    struct WriterStateMachine<T> : IAsyncStateMachine
    {
        public ILucentObjectWriter Writer;
        public int State;
        public AsyncTaskMethodBuilder AsyncBuilder;
        public T Instance;

        public Func<T, ILucentObjectWriter, ulong, TaskAwaiter> AwaiterMap;

        public void MoveNext()
        {
            try
            {
                while (true)
                {
                    var aw = AwaiterMap(Instance, Writer, (ulong)State);
                    if (aw.Equals(default(TaskAwaiter)))
                        break;

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