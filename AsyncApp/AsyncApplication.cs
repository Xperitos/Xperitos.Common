using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace Xperitos.Common.AsyncApp
{
    /// <summary>
    /// Derive to implement a message loop.
    /// </summary>
    public abstract class AsyncApplication : ISyncContextProvider
    {
        protected AsyncApplication()
        {
            m_syncContext = new SingleThreadSynchronizationContext();
            m_syncContextScheduler = new SynchronizationContextScheduler(m_syncContext);
        }

        /// <summary>
        /// Return the SyncContext for the current thread.
        /// </summary>
        public static SynchronizationContext CurrentSyncContext { get { return m_currentSyncContext.Value; } }

        private static readonly ThreadLocal<SynchronizationContext> m_currentSyncContext = new ThreadLocal<SynchronizationContext>();

        /// <summary>
        /// The sync context associated with THIS application instance.
        /// </summary>
        public SynchronizationContext SyncContext { get { return m_syncContext; } }

        /// <summary>
        /// Start pumping messages. Set synchronization context on the way (for async tasks / RX to work).
        /// </summary>
        public void Run()
        {
            var prevCtx = SynchronizationContext.Current;
            var prevSyncContext = m_currentSyncContext.Value;

            try
            {
                // Establish the new context
                SynchronizationContext.SetSynchronizationContext(m_syncContext);

                // Set scheduler.
                m_currentSyncContext.Value = m_syncContext;

                // Init stuff.
                if (!OnInit())
                    return;

                // Start pumping.
                m_syncContext.RunOnCurrentThread();
            }
            finally
            {
                m_currentSyncContext.Value = prevSyncContext;
                SynchronizationContext.SetSynchronizationContext(prevCtx);
            }
        }

        private readonly SingleThreadSynchronizationContext m_syncContext;
        private readonly SynchronizationContextScheduler m_syncContextScheduler;

        /// <summary>
        /// Signal the message loop to terminate.
        /// </summary>
        public void Quit()
        {
            // Signal the loop to quit.
            m_syncContext.Post((o) => OnExitAsync().ContinueWith(oo => m_syncContext.Complete()), null);
            ;
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
    }
}
