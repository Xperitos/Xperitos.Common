using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Xperitos.Common.AsyncApp.Impl;

namespace Xperitos.Common.AsyncApp
{
    /// <summary>
    /// Use this class to create a threaded sync context.
    /// Start it using the <see cref="Start"/> function below. Stop by calling <see cref="Dispose"/> on the object.
    /// </summary>
    public sealed class ThreadedMessageLoop : IDisposable, ISyncContextProvider
    {
        /// <summary>
        /// Construct the message loop.
        /// </summary>
        /// <param name="onStart">Action to run when loop starts (inside the message loop)</param>
        /// <param name="onExitAsync">Action to run when loop terminates (inside the message loop). Should return an async task which completes when terminated</param>
        /// <param name="threadName">Thread name if specified</param>
        /// <param name="isBackground">Is the thread a background thread?</param>
        /// <returns>Object when disposed, the message loop will initiate termination sequence</returns>
        public ThreadedMessageLoop(Action onStart = null, Func<Task> onExitAsync = null, bool isBackground = true, string threadName = null)
        {
            m_threadName = threadName;
            m_isBackground = isBackground;
            m_instance = new MessageLoopDelegate();
            m_instance.SetOnExitAsync(onExitAsync);
            if (onStart != null)
                m_instance.SetOnStart(() =>
                {
                    onStart();
                    return true;
                });
        }

        private readonly string m_threadName;
        private readonly bool m_isBackground;

        /// <summary>
        /// Start the message loop thread.
        /// </summary>
        public void Start()
        {
            if (m_instance == null)
                throw new ObjectDisposedException(typeof(ThreadedMessageLoop).Name);

            if (m_thread != null)
                throw new InvalidOperationException("Already started");

            m_thread = new Thread(m_instance.Run)
            {
                Name = m_threadName ?? "ThreadedMessageLoop thread",
                IsBackground = m_isBackground
            };

            // Start running.
            m_thread.Start();
        }

        /// <summary>
        /// Ask the message loop to terminate.
        /// </summary>
        public Task StopAsync()
        {
            // Signal the application to quit.
            return m_instance?.QuitAsync() ?? Task.FromResult(true);
        }

        /// <summary>
        /// Wait for the message loop to terminate.
        /// </summary>
        public void Join()
        {
            m_thread?.Join();
        }

        private MessageLoopDelegate m_instance;
        private Thread m_thread;

        /// <summary>
        /// Terminate and dispose the message loop.
        /// </summary>
        /// <remarks>This function blocks until the message loop terminates</remarks>
        public void Dispose()
        {
            StopAsync();
            Join();
            m_instance = null;
            m_thread = null;
        }

        public SynchronizationContext SyncContext => m_instance.SyncContext;
    }
}
