using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
                throw new ArgumentNullException(nameof(d));

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
        public void RunOnCurrentThread(CancellationToken ct = default(CancellationToken))
        {
            if (m_thread != null)
                throw new InvalidOperationException("Already running!");

            m_thread = Thread.CurrentThread;

            try
            {
                foreach (var workItem in m_queue.GetConsumingEnumerable(ct))
                    workItem.Key(workItem.Value);
            }
            catch (OperationCanceledException e)
            {
                // Ignore cancellation originating from our token.
                if (!ct.IsCancellationRequested)
                {
                    this.Log().DebugException("Unhandled exception!", e);
                    throw;
                }
            }
            catch (Exception e)
            {
                this.Log().DebugException("Unhandled exception!", e);
                throw;
            }
            finally
            {
                m_thread = null;
            }
        }

        public void ProcessSingleItem()
        {
            KeyValuePair<SendOrPostCallback, object> item;
            if (m_queue.TryTake(out item))
                item.Key(item.Value);
        }

        /// <summary>
        /// Returns the current queue length.
        /// </summary>
        public int QueueLength => m_queue.Count;

        /// <summary>
        /// The number of ongoing incomplete operations.
        /// </summary>
        public int RunningOperationsCount => m_runningOperationsCount;

        private int m_runningOperationsCount;

        public override void OperationStarted()
        {
            Interlocked.Increment(ref m_runningOperationsCount);
            base.OperationStarted();
        }

        public override void OperationCompleted()
        {
            base.OperationCompleted();
            Interlocked.Decrement(ref m_runningOperationsCount);
        }
    }
}