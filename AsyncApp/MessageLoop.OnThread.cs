using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Constructor usage for backwards compatibility with this function.
            var messageLoop = new ThreadedMessageLoop(onStart, onExitAsync, false, "MessageLoop thread");

            // Start running automatically.
            messageLoop.Start();
            return messageLoop;
        }
    }
}
