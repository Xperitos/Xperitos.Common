using System;
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
        public static Task<T> SendAsync<T>(this SynchronizationContext context, Func<Task<T>> action)
        {
            var result = new TaskCompletionSource<T>();
            context.Post(
                async (taskCompletion) => ((TaskCompletionSource<T>)taskCompletion).SetResult(await action()),
                result);
            return result.Task;
        }

        /// <summary>
        /// Performs the action async on the specified context and returns a task.
        /// </summary>
        public static Task SendAsync(this SynchronizationContext context, Action action)
        {
            return context.SendAsync(() => {action(); return Task.FromResult(true);});
        }

        public static void Post(this SynchronizationContext context, Action action)
        {
            context.Post((o) => action(), null);
        }
    }
}
