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

        private class SynchronizeInvoke : ISynchronizeInvoke
        {
            public SynchronizeInvoke(SynchronizationContext ctx)
            {
                m_ctx = ctx;
            }

            private readonly SynchronizationContext m_ctx;

            public IAsyncResult BeginInvoke(Delegate method, object[] args)
            {
                var task = m_ctx.SendTaskAsync(() => Task.FromResult(method.DynamicInvoke(args)));
                return task;
            }

            public object EndInvoke(IAsyncResult result)
            {
                var task = (Task<object>)result;
                return task.Result;
            }

            public object Invoke(Delegate method, object[] args)
            {
                return m_ctx.Send(() => method.DynamicInvoke(args));
            }

            public bool InvokeRequired
            {
                get
                {
                    // No way of telling from the context if invoke is requred or not so default to true.
                    return true;
                }
            }
        }

        public static ISynchronizeInvoke GetSynchronizeInvoke(this SynchronizationContext context)
        {
            return new SynchronizeInvoke(context);
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
                        tcs.SetResult(await action());
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
