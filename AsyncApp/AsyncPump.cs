using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Original from http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx.

namespace Xperitos.Common.AsyncApp
{

    /// <summary>Represents a pump that runs an asynchronous method and all its continuations on the current thread.
    /// </summary>
    /// <remarks>Some asynchronous methods expect that all its continuations are executed on the same thread. If such
    /// code needs to be run in an environment where this is not guaranteed
    /// (<see cref="SynchronizationContext.Current"/> is either <c>null</c> or is a
    /// <see cref="SynchronizationContext"/> object that schedules continuations on different threads as under ASP.NET)
    /// then this class can be used to force execution on a single thread.</remarks>
    /// <threadsafety static="true" instance="false" />
    public static class AsyncPump
    {
        /// <summary>Runs <paramref name="asyncMethod"/> on the current thread.</summary>
        public static void Run(Func<Task> asyncMethod)
        {
            Run(() => asyncMethod().ContinueWith(t => true));
        }

        /// <summary>Runs <paramref name="asyncMethod"/> on the current thread.</summary>
        public static T Run<T>(Func<Task<T>> asyncMethod)
        {
            if (asyncMethod == null)
                throw new ArgumentNullException(nameof(asyncMethod));

            var previousContext = SynchronizationContext.Current;
            using (var newContext = new SingleThreadSynchronizationContext())
            {
                SynchronizationContext.SetSynchronizationContext(newContext);
                try
                {
                    newContext.OperationStarted();

                    // Get the initial task.
                    var task = asyncMethod();

                    if (task == null)
                    {
                        newContext.OperationCompleted();
                        throw new ArgumentException("The method returned a null task.", nameof(asyncMethod));
                    }

                    // When the task completes, signal the inner context about the completion.
                    task.ContinueWith(t => newContext.OperationCompleted(), TaskScheduler.Default);

                    // Pump messages.
                    newContext.RunOnCurrentThread();

                    // Force the task into completion state.
                    return task.GetAwaiter().GetResult();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(previousContext);
                }
            }
        }

        private sealed class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable
        {
            private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> m_queue =
                new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

            private int m_operationCount;

            public void Dispose()
            {
                m_queue.Dispose();
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }

            public override void OperationStarted()
            {
                Interlocked.Increment(ref m_operationCount);
            }

            public override void OperationCompleted()
            {
                if (Interlocked.Decrement(ref m_operationCount) == 0)
                    m_queue.CompleteAdding();
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                if (d == null)
                    throw new ArgumentNullException(nameof(d));

                m_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("Send is not supported.");
            }

            public void RunOnCurrentThread()
            {
                foreach (var workItem in m_queue.GetConsumingEnumerable())
                    workItem.Key(workItem.Value);
            }
        }
    }
}