using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Xperitos.Common.AsyncApp;

namespace Xperitos.Common.Utils
{
    public static class SyncContextProviderMixins
    {
        public static IScheduler GetScheduler(this ISyncContextProvider context)
        {
            return SynchronizationContextMixins.GetScheduler(context.SyncContext);
        }

        /// <summary>
        /// Performs the action on the specified context and blocks the current thread until a result returns.
        /// </summary>
        public static T Send<T>(this ISyncContextProvider context, Func<T> action)
        {
            return SynchronizationContextMixins.Send(context.SyncContext, action);
        }

        /// <summary>
        /// Performs the action async on the specified context and returns a task.
        /// </summary>
        public static Task<T> SendAsync<T>(this ISyncContextProvider context, Func<T> action)
        {
            return SynchronizationContextMixins.SendAsync<T>(context.SyncContext, action);
        }

        /// <summary>
        /// Performs the action async on the specified context and returns a task.
        /// </summary>
        public static Task SendAsync(this ISyncContextProvider context, Action action)
        {
            return SynchronizationContextMixins.SendAsync(context.SyncContext, action);
        }

        public static void Post(this ISyncContextProvider context, Action action)
        {
            SynchronizationContextMixins.Post(context.SyncContext, action);
        }
    }
}