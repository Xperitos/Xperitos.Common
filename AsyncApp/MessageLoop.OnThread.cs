using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xperitos.Common.AsyncApp.Impl;

namespace Xperitos.Common.AsyncApp
{
    public partial class MessageLoop
    {
        /// <summary>
        /// Use this function to start a message loop in a different thread.
        /// </summary>
        /// <param name="onStart">Action to run when loop starts (inside the message loop)</param>
        /// <param name="onExitAsync">Action to run when loop terminates (inside the message loop). Should return an async task which completes when terminated</param>
        /// <returns>Object when disposed, the message loop will initiate termination sequence</returns>
        public static IDisposable CreateMessageLoopThread(Action onStart = null, Func<Task> onExitAsync = null)
        {
            var appInstance = new MessageLoopApp();

            appInstance.SetOnExitAsync(onExitAsync);
            if (onStart != null)
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
                appInstance.QuitAsync();

                // Wait for the thread to terminate.
                thread.Join();
            });

            // Start running.
            thread.Start();
            return terminateDisposable;
        }
    }
}
