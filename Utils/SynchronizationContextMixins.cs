using System;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    public static class SynchronizationContextMixins
    {
        public static IScheduler GetScheduler(this SynchronizationContext context)
        {
            return new SynchronizationContextScheduler(context);
        }

        /// <summary>
        /// Performs the action on the specified context and blocks the current thread until a result returns.
        /// </summary>
        public static T Send<T>(this SynchronizationContext context, Func<T> action)
        {
            T result = default(T);
            context.Send((o) => { result = action(); }, null);
            return result;
        }

        /// <summary>
        /// Performs the action async on the specified context and immediately returns a task for it.
        /// </summary>
        [Obsolete("Use SendTaskAsync")]
        public static Task<T> SendAsync<T>(this SynchronizationContext context, Func<Task<T>> action)
        {
            return SendTaskAsync(context, action);
        }

        /// <summary>
        /// Performs the action async on the specified context and returns a task.
        /// </summary>
        [Obsolete("Use SendActionAsync")]
        public static Task SendAsync(this SynchronizationContext context, Action action)
        {
            return SendActionAsync(context, action);
        }

        /// <summary>
        /// Performs the async action on the specified context and returns a task..
        /// </summary>
        public static Task<T> SendTaskAsync<T>(this SynchronizationContext context, Func<Task<T>> action)
        {
            var result = new TaskCompletionSource<T>();
            context.Post(
                async (taskCompletion) =>
                {
                    var tcs = (TaskCompletionSource<T>)taskCompletion;
                    try
                    {
                        tcs.SetResult(await action().ConfigureAwait(false));
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }
                },
                result);
            return result.Task;
        }

        /// <summary>
        /// Performs the action async on the specified context and returns a task.
        /// </summary>
        public static Task<T> SendActionAsync<T>(this SynchronizationContext context, Func<T> action)
        {
            var tcs = new TaskCompletionSource<T>();
            context.Post(() =>
            {
                try
                {
                    var result = action();
                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// Performs the action async on the specified context and returns a task.
        /// </summary>
        public static Task SendActionAsync(this SynchronizationContext context, Action action)
        {
            return context.SendActionAsync(() => { action(); return true; });
        }

        public static void Post(this SynchronizationContext context, Action action)
        {
            context.Post((o) => action(), null);
        }
    }
}
