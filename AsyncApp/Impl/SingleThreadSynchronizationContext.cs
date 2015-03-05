using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Xperitos.Common.AsyncApp.Impl
{
    /// <summary>Provides a SynchronizationContext that's single-threaded.</summary>
    sealed class SingleThreadSynchronizationContext : SynchronizationContext
    {
        /// <summary>The queue of work items.</summary>
        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> m_queue =
            new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        /// <summary>The processing thread.</summary>
        private readonly Thread m_thread = Thread.CurrentThread;

        /// <summary>Dispatches an asynchronous message to the synchronization context.</summary>
        /// <param name="d">The System.Threading.SendOrPostCallback delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        public override void Post(SendOrPostCallback d, object state)
        {
            if (d == null) 
                throw new ArgumentNullException("d");

            if (m_queue.IsCompleted)
                return;

            m_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
        }

        /// <summary>Not supported.</summary>
        public override void Send(SendOrPostCallback d, object state)
        {
            if (m_thread == Thread.CurrentThread)
                d(state);
            else
            {
                using (var waitEvent = new ManualResetEventSlim())
                {
                    SendOrPostCallback func = (o) =>
                    {
                        d(state);
                        // ReSharper disable AccessToDisposedClosure
                        waitEvent.Set();
                        // ReSharper restore AccessToDisposedClosure
                    };
                    Post(func, null);
                    waitEvent.Wait();
                }
            }
        }

        /// <summary>Runs an loop to process all queued work items.</summary>
        public void RunOnCurrentThread()
        {
            foreach (var workItem in m_queue.GetConsumingEnumerable())
                workItem.Key(workItem.Value);
        }

        /// <summary>Notifies the context that no more work will arrive.</summary>
        public void Complete() { m_queue.CompleteAdding(); }
    }
}