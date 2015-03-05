using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace Xperitos.Common.AsyncApp
{
    public static class MessageLoop
    {
        /// <summary>
        /// Current RX scheduler for the current thread.
        /// </summary>
        public static IScheduler Scheduler { get { return AsyncApplication.CurrentSyncContext.Scheduler; } }

        /// <summary>
        /// Current sync context for the current thread.
        /// </summary>
        public static ISyncContext SyncContext { get { return AsyncApplication.CurrentSyncContext; } }

        /// <summary>
        /// Use this function to start a message loop in a different thread.
        /// </summary>
        /// <param name="onStart">Action to run when loop starts (inside the message loop)</param>
        /// <param name="onExitAsync">Action to run when loop terminates (inside the message loop). Should return an async task which completes when terminated</param>
        /// <returns>Object when disposed, the message loop will initiate termination sequence</returns>
        public static IDisposable RunThread(Action onStart = null, Func<Task> onExitAsync = null)
        {
            var appInstance = new MessageLoopApp();

            appInstance.SetOnExitAsync(onExitAsync);
            if ( onStart != null )
                appInstance.SetOnStart(() =>
                                       {
                                           onStart();
                                           return true;
                                       });

            var thread = new Thread(appInstance.Run)
                         {
                             Name = "MessageLoop thread"
                         };
            var terminateDisposable = Disposable.Create(() =>
                                                        {
                                                            // Signal the application to quit.
                                                            appInstance.Quit();

                                                            // Wait for the thread to terminate.
                                                            thread.Join();
                                                        });

            // Start running.
            thread.Start();
            return terminateDisposable;
        }

        /// <summary>
        /// Use this function to start a message loop.
        /// </summary>
        /// <param name="onStart">Action to run when loop starts (inside the message loop) - it receives a disposable, when disposed it terminates the message loop</param>
        /// <param name="onExitAsync">Action to run when loop terminates (inside the message loop). Should return an async task which completes when terminated</param>
        public static void RunSync(Func<IDisposable, bool> onStart, Func<Task> onExitAsync = null)
        {
            var appInstance = new MessageLoopApp();
            var terminateDisposable = Disposable.Create(appInstance.Quit);

            appInstance.SetOnExitAsync(onExitAsync);
            appInstance.SetOnStart(() => onStart(terminateDisposable));
            appInstance.Run();
        }

        class MessageLoopApp : AsyncApplication
        {
            public void SetOnStart(Func<bool> onStart)
            {
                m_onStart = onStart;
            }

            public void SetOnExitAsync(Func<Task> onExit)
            {
                m_onExitAsync = onExit;
            }

            private Func<bool> m_onStart;
            private Func<Task> m_onExitAsync;

            #region Overrides of AsyncApplication

            protected override bool OnInit()
            {
                if ( m_onStart == null )
                    return true;

                return m_onStart();
            }

            protected override Task OnExitAsync()
            {
                if ( m_onExitAsync != null )
                    return m_onExitAsync();

                return Task.FromResult(true);
            }

            #endregion

            #region Implementation of IDisposable

            public void Dispose()
            {
                Quit();
            }

            #endregion
        }
    }
}