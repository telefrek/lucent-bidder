using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Lucent.Common.Serialization._Internal
{
    /// <summary>
    /// Implementation of an async state machine for arrays
    /// </summary>
    /// <remarks>
    /// Need this to create asynchronous readers/writers via Expressions + Lambdas
    /// </remarks>
    [CompilerGenerated]
    [StructLayout(LayoutKind.Auto)]
    struct ArrayWriterStateMachine<T> : IAsyncStateMachine
    {
        public ILucentArrayWriter Writer;
        public ISerializationContext Context;
        public int State;
        public AsyncTaskMethodBuilder AsyncBuilder;
        public T[] Instances;
        public Func<T, ILucentArrayWriter, ISerializationContext, TaskAwaiter> WriteObj;
        bool Finished;
        bool Flushed;

        public void MoveNext()
        {
            try
            {
                // Keep writing the objects
                while (State < Instances.Length)
                {
                    var aw = WriteObj(Instances[State++], Writer, Context);
                    if (!aw.IsCompleted)
                    {
                        AsyncBuilder.AwaitUnsafeOnCompleted(ref aw, ref this);
                        return;
                    }
                }

                if (State == Instances.Length)
                {
                    var aw = Writer.WriteEnd().GetAwaiter();
                    State++;
                    if (!aw.IsCompleted)
                    {
                        AsyncBuilder.AwaitUnsafeOnCompleted(ref aw, ref this);
                        return;
                    }
                }

                if (State == Instances.Length + 1)
                {
                    var aw = Writer.Flush().GetAwaiter();
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