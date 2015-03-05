using System.Reactive.Concurrency;
using System.Threading;

namespace Xperitos.Common.AsyncApp
{
    public interface ISyncContext
    {
        /// <summary>
        /// The scheduler associated with THIS application instance.
        /// </summary>
        IScheduler Scheduler { get; }

        /// <summary>
        /// The sync context associated with THIS application instance.
        /// </summary>
        SynchronizationContext SyncContext { get; }
    }
}