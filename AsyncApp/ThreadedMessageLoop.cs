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

            if (m_terminateDisposable != null)
                throw new InvalidOperationException("Already started");

            var thread = new Thread(m_instance.Run)
            {
                Name = m_threadName ?? "ThreadedMessageLoop thread",
                IsBackground = m_isBackground
            };

            m_terminateDisposable = Disposable.Create(() =>
            {
                // Signal the application to quit.
                m_instance.QuitAsync();

                // Wait for the thread to terminate.
                thread.Join();
            });

            // Start running.
            thread.Start();
        }

        private MessageLoopDelegate m_instance;
        private IDisposable m_terminateDisposable;

        public void Dispose()
        {
            if (m_terminateDisposable == null)
                return;

            m_terminateDisposable.Dispose();
            m_terminateDisposable = null;
            m_instance = null;
        }

        public SynchronizationContext SyncContext => m_instance.SyncContext;
    }
}