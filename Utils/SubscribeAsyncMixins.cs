using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    public static class SubscribeAsyncMixins
    {
        /// <summary>
        /// Serialize the calls to the action callback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable">Sequence of events</param>
        /// <param name="action">Async action to run for each event.</param>
        /// <param name="scheduler">Where the action execution takes place</param>
        /// <param name="maxPendingEvents">If queued events exceeds this much items, and overflow exception is thrown</param>
        /// <returns></returns>
        public static IDisposable SubscribeAsync<T>(this IObservable<T> observable, Func<T, CancellationToken, Task> action, IScheduler scheduler = null, int maxPendingEvents = 100)
        {
            if (scheduler == null)
                scheduler = Scheduler.Default;

            var cts = new CancellationDisposable();

            // Queue of items + sync object.
            var pendingEvents = new Queue<T>();

            var syncSemaphoreSlim = new SemaphoreSlim(1);

            observable.Subscribe(v =>
            {
                Action processPendingItems = async () =>
                {
                    // Allow only ONE function to run async.
                    using (await syncSemaphoreSlim.DoLockAsync())
                    {
                        while (!cts.IsDisposed)
                        {
                            // Dequeue inside the lock.
                            T item;
                            lock (pendingEvents)
                            {
                                if (pendingEvents.Count == 0)
                                    return;

                                item = pendingEvents.Dequeue();
                            }

                            // Process the item!
                            await action(item, cts.Token);
                        }
                    }
                };

                // Enqueue.
                lock (pendingEvents)
                {
                    if (pendingEvents.Count > maxPendingEvents)
                        throw new OverflowException("Pending events exceed max allowed size");

                    pendingEvents.Enqueue(v);

                    scheduler.Schedule(processPendingItems);
                }
            }, cts.Token);

            return cts;
        }
    }
}
