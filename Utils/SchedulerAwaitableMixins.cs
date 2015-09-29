using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    public static class SchedulerAwaitableMixins
    {
        /// <summary>
        /// Runs the given task under a specific scheduler and returns a task for it.
        /// </summary>
        public static Task<T> ScheduleAwaitable<T>(this IScheduler scheduler, Func<T> action, CancellationToken ct = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<T>();
            var disposable = scheduler.Schedule(() => tcs.SetResult(action()));

            // Dispose the scheduled action when ct is cancelled.
            ct.Register(disposable.Dispose);

            return tcs.Task;
        }

        /// <summary>
        /// Runs the given task under a specific scheduler and returns a task for it.
        /// </summary>
        public static Task<T> ScheduleAwaitable<T>(this IScheduler scheduler, Func<Task<T>> action, CancellationToken ct = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<T>();
            var disposable = scheduler.Schedule(async () => tcs.SetResult(await action()));

            // Dispose the scheduled action when ct is cancelled.
            ct.Register(disposable.Dispose);

            return tcs.Task;
        }

        /// <summary>
        /// Runs the given task under a specific scheduler and returns a task for it.
        /// </summary>
        public static Task ScheduleAwaitable(this IScheduler scheduler, Func<Task> action, CancellationToken ct = default(CancellationToken))
        {
            return ScheduleAwaitable(scheduler, async () => { await action(); return true; }, ct);
        }

        /// <summary>
        /// Runs the given task under a specific scheduler and returns a task for it.
        /// </summary>
        public static Task ScheduleAwaitable(this IScheduler scheduler, Action action, CancellationToken ct = default(CancellationToken))
        {
            return ScheduleAwaitable(scheduler, () => { action(); return true; }, ct);
        }
    }
}
