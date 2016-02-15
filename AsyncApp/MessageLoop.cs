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
                var result = m_currentMessageLoop.Value;
                if (result == null)
                    throw new InvalidOperationException("No message loop for current thread!");
                return result;
            } 
        }
        private static readonly ThreadLocal<MessageLoop> m_currentMessageLoop = new ThreadLocal<MessageLoop>();

        private readonly SingleThreadSynchronizationContext m_syncContext;
        public SynchronizationContext SyncContext { get { return m_syncContext; } }
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
            if (m_currentMessageLoop.Value != null)
                throw new InvalidOperationException("Already running on current thread");

            var prevCtx = SynchronizationContext.Current;

            try
            {
                // Establish the new context
                SynchronizationContext.SetSynchronizationContext(m_syncContext);

                // Set scheduler.
                m_currentMessageLoop.Value = this;

                // Init stuff.
                if (!OnInit())
                    return;

                // Start pumping.
                m_syncContext.RunOnCurrentThread();
            }
            finally
            {
                m_currentMessageLoop.Value = null;
                SynchronizationContext.SetSynchronizationContext(prevCtx);
            }
        }

        /// <summary>
        /// Return the current number of pending messages in the queue.
        /// </summary>
        public int CurrentMessageQueueLength { get { return m_syncContext.QueueLength; } }

        private Task m_quitTask;
        private readonly object m_quitTaskLock = new object();

        /// <summary>
        /// Signal the message loop to terminate.
        /// </summary>
        public Task QuitAsync()
        {
            TaskCompletionSource<bool> completion;

            // Make sure only a single quit task exists.
            lock (m_quitTaskLock)
            {
                if (m_quitTask != null)
                    return m_quitTask;

                completion = new TaskCompletionSource<bool>();
                m_quitTask = completion.Task;
            }

            // Signal the loop to quit.
            m_syncContext.Post((o) => OnExitAsync().ContinueWith(oo => { m_syncContext.Complete();completion.SetResult(true); }), null);

            return m_quitTask;
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
            if (m_quitTask != null)
                return;

            QuitAsync();
        }
    }
}
