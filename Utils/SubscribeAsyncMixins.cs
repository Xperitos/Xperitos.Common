using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    public static class SubscribeAsyncMixins
    {
        /// <summary>
        /// Serialize the calls to the action callback - the action call will be re-scheduled only once if multiple events are received while it was still running.
        /// </summary>
        /// <param name="observable">Sequence of events</param>
        /// <param name="action">Async action to run for each event.</param>
        /// <param name="scheduler">Where the action execution takes place</param>
        public static IDisposable SubscribeSingleAsync(this IObservable<Unit> observable, Func<CancellationToken, Task> action, IScheduler scheduler)
        {
            if (scheduler == null)
                scheduler = Scheduler.Default;

            // Queue of items + sync object.
            var runActionEvent = new ManualResetEventSlim(false);
            var disposables = new CompositeDisposable();

            // Create the pump loop.
            scheduler.ScheduleAsync(async (innerScheduler, ct) =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        // Wait for a request to run the action.
                        await Task.Run(() => runActionEvent.Wait(ct), ct);

                        // Clear the event before the action is executed.
                        runActionEvent.Reset();

                        // Perform the action.
                        await action(ct);
                    }
                    catch (OperationCanceledException)
                    {
                        // Catch cancelled exception (but only if it's ours).
                        if (!ct.IsCancellationRequested)
                            throw;
                    }

                    // Yield to the scheduler.
                    await innerScheduler.Yield();
                }
            }).ComposeDispose(disposables);

            // Subscribe to events.
            observable
                .Subscribe( 
                    v => runActionEvent.Set(),
                    e => disposables.Dispose(),
                    () => disposables.Dispose())
                .ComposeDispose(disposables);

            return disposables;
        }

        /// <summary>
        /// Serialize the calls to the action callback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable">Sequence of events</param>
        /// <param name="action">Async action to run for each event.</param>
        /// <param name="scheduler">Where the action execution takes place</param>
        /// <param name="maxPendingEvents">If queued events exceeds this much items, and overflow exception is thrown</param>
        public static IDisposable SubscribeAsync<T>(this IObservable<T> observable, Func<T, CancellationToken, Task> action, IScheduler scheduler, int maxPendingEvents = 100)
        {
            if (scheduler == null)
                scheduler = Scheduler.Default;

            // Queue of items + sync object.
            var pendingEvents = new Queue<T>();
            var countSemaphore = new SemaphoreSlim(0);

            var disposables = new CompositeDisposable();

            // Create the pump loop.
            scheduler.ScheduleAsync(async (innerScheduler, ct) =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        T item;

                        // Wait for item to become available.
                        await countSemaphore.WaitAsync(ct);
                        lock (pendingEvents)
                            item = pendingEvents.Dequeue();

                        // Perform the action.
                        await action(item, ct);
                    }
                    catch (OperationCanceledException)
                    {
                        // Catch cancelled exception (but only if it's ours).
                        if (!ct.IsCancellationRequested)
                            throw;
                    }

                    // Yield to the scheduler.
                    await innerScheduler.Yield();
                }
            }).ComposeDispose(disposables);

            // Subscribe to events.
            observable
                .Subscribe(
                    v =>
                    {
                        lock (pendingEvents)
                        {
                            if (pendingEvents.Count > maxPendingEvents)
                                throw new OverflowException("Pending events exceed max allowed size");

                            pendingEvents.Enqueue(v);
                            countSemaphore.Release();
                        }
                    }, 
                    e => disposables.Dispose(), 
                    () => disposables.Dispose())
                .ComposeDispose(disposables);

            return disposables;
        }
    }
}
