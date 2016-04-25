using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Xperitos.Common.AsyncApp.Impl;

namespace Xperitos.Common.AsyncApp
{
    /// <summary>
    /// Base class for a message loop.
    /// </summary>
    public abstract partial class MessageLoop : IDisposable, ISyncContextProvider
    {
        /// <summary>
        /// Time to give the message pump to terminate gracefully (all queued items to be executed).
        /// </summary>
        private static readonly TimeSpan TerminationTimeout = TimeSpan.FromSeconds(15);


        internal protected MessageLoop()
        {
            m_syncContext = new SingleThreadSynchronizationContext();
        }

        /// <summary>
        /// Return the SyncContext for the current thread.
        /// </summary>
        public static MessageLoop Current 
        {
            get
            {
                var result = CurrentMessageLoopTLS.Value;
                if (result == null)
                    throw new InvalidOperationException("No message loop for current thread!");
                return result;
            } 
        }
        private static readonly ThreadLocal<MessageLoop> CurrentMessageLoopTLS = new ThreadLocal<MessageLoop>();

        private readonly SingleThreadSynchronizationContext m_syncContext;
        public SynchronizationContext SyncContext => m_syncContext;

        public static explicit operator SynchronizationContext(MessageLoop loop)
        {
            return loop.SyncContext;
        }

        public bool IsOperationRunning => m_syncContext.RunningOperationsCount > 0;
        public int RunningOperationsCount => m_syncContext.RunningOperationsCount;

        /// <summary>
        /// Start pumping messages. Set synchronization context on the way (for async tasks / RX to work).
        /// </summary>
        public void Run()
        {
            if (CurrentMessageLoopTLS.Value != null)
                throw new InvalidOperationException("Already running on current thread");

            if (m_quitCancellationSource.IsCancellationRequested)
                throw new InvalidOperationException("MessageLoop disposed");

            var prevCtx = SynchronizationContext.Current;

            try
            {
                // Establish the new context
                SynchronizationContext.SetSynchronizationContext(m_syncContext);

                // Set scheduler.
                CurrentMessageLoopTLS.Value = this;

                // Init stuff.
                if (!OnInit())
                    return;

                // Start pumping.
                m_syncContext.RunOnCurrentThread(m_quitCancellationSource.Token);

                //
                // Shutdown sequence.
                //

                // Mark start time.
                DateTimeOffset terminationStartTime = DateTimeOffset.Now;

                // Schedule the "Exit" task execution.
                m_syncContext.Post(async o => await OnExitAsync(), null);

                // Pump all the pending messages or until the timeout was reached.
                while (m_syncContext.QueueLength > 0)
                {
                    if (DateTimeOffset.Now - terminationStartTime >= TerminationTimeout)
                        throw new TimeoutException($"MessageLoop termination process took too long. Pending queue items: {m_syncContext.QueueLength}");

                    // Process an item.
                    m_syncContext.ProcessSingleItem();
                }

                m_quitTaskSource.TrySetResult(true);
            }
            finally
            {
                CurrentMessageLoopTLS.Value = null;
                SynchronizationContext.SetSynchronizationContext(prevCtx);
            }
        }

        /// <summary>
        /// Return the current number of pending messages in the queue.
        /// </summary>
        public int CurrentMessageQueueLength => m_syncContext.QueueLength;

        private readonly CancellationTokenSource m_quitCancellationSource = new CancellationTokenSource();
        private readonly TaskCompletionSource<bool> m_quitTaskSource = new TaskCompletionSource<bool>();

        /// <summary>
        /// Signal the message loop to terminate.
        /// </summary>
        public Task QuitAsync()
        {
            m_quitCancellationSource.Cancel();
            return m_quitTaskSource.Task;
        }

        /// <summary>
        /// Initialization function.
        /// </summary>
        protected abstract bool OnInit();

        /// <summary>
        /// Called to cleanup.
        /// </summary>
        protected virtual Task OnExitAsync()
        {
            // Noop.
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            QuitAsync();
        }
    }
}
