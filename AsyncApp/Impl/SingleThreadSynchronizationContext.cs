using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Splat;

namespace Xperitos.Common.AsyncApp.Impl
{
    /// <summary>Provides a SynchronizationContext that's single-threaded.</summary>
    sealed class SingleThreadSynchronizationContext : SynchronizationContext, IEnableLogger
    {
        /// <summary>The queue of work items.</summary>
        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> m_queue =
            new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        /// <summary>The processing thread.</summary>
        private Thread m_thread = null;

        /// <summary>Dispatches an asynchronous message to the synchronization context.</summary>
        /// <param name="d">The System.Threading.SendOrPostCallback delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        public override void Post(SendOrPostCallback d, object state)
        {
            if (d == null) 
                throw new ArgumentNullException("d");

            if (m_queue.IsCompleted)
            {
                // Queue is closed - log the error and run in-place.
                // It's needed to prevent dead-locking when trying to queue task results while the process terminates.
                this.Log().Error("Synchronization context terminated - running action in-place");
                d(state);
                return;
            }

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
            m_thread = Thread.CurrentThread;

            try
            {
                foreach (var workItem in m_queue.GetConsumingEnumerable())
                    workItem.Key(workItem.Value);
            }
            catch (Exception e)
            {
                // Going down - prevent addition of items to the queue.
                m_queue.CompleteAdding();

                this.Log().DebugException("Unhandled exception!", e);
                throw;
            }
        }

        /// <summary>Notifies the context that no more work will arrive.</summary>
        public void Complete() { m_queue.CompleteAdding(); }

        /// <summary>
        /// Returns the current queue length.
        /// </summary>
        public int QueueLength { get { return m_queue.Count; } }
    }
}