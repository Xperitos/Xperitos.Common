using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xperitos.Common.Utils;

namespace Xperitos.Common.AsyncApp.Reactive
{
    public class MessageLoopScheduler : SynchronizationContextScheduler
    {
        public MessageLoopScheduler(MessageLoop messageLoop) :
            base(messageLoop.SyncContext)
        {
            MessageLoop = messageLoop;
        }

        /// <summary>
        /// The associated message loop scheduler.
        /// </summary>
        public MessageLoop MessageLoop { get; private set; }

        /// <summary>
        /// Return the SyncContext for the current thread.
        /// </summary>
        public static MessageLoopScheduler Current => new MessageLoopScheduler(MessageLoop.Current);
    }
}
