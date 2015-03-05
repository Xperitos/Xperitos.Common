using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    public static class SynchronizationContextMixins
    {
        public static void Post(this SynchronizationContext context, Action action)
        {
            context.Post((o) => action(), null);
        }

        /// <summary>
        /// Run the specified function on the specified context and return a task for it.
        /// </summary>
        public static Task<T> PostAsync<T>(this SynchronizationContext context, Func<Task<T>> action)
        {
            var result = new TaskCompletionSource<T>();
            context.Post(
                async (taskCompletion) => ((TaskCompletionSource<T>)taskCompletion).SetResult(await action()), 
                result);

            return result.Task;
        }

        /// <summary>
        /// Run the specified function on the specified context and return a task for it.
        /// </summary>
        public static Task PostAsync(this SynchronizationContext context, Func<Task> action)
        {
            return PostAsync(context, async () => {await action(); return true; });
        }
    }
}
