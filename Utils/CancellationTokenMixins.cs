using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    public static class CancellationTokenMixins
    {
        /// <summary>
        /// Automatically convert the concellation token to a waitable task.
        /// </summary>
        public static TaskAwaiter GetAwaiter(this CancellationToken token)
        {
            TaskCompletionSource<bool> cts = new TaskCompletionSource<bool>();
            var reg = token.Register(() => cts.SetResult(true));
            return ((Task)cts.Task).GetAwaiter();
        }

        /// <summary>
        /// Transform the cancellation token to a waitable task, providing the specified cancellation token to cancel the task.
        /// </summary>
        /// <param name="waitForToken">Token to transform to a task</param>
        /// <param name="ct">Cancellation token</param>
        public static Task ToTask(this CancellationToken waitForToken, CancellationToken ct)
        {
            TaskCompletionSource<bool> cts = new TaskCompletionSource<bool>();
            var reg = waitForToken.Register(() => cts.SetResult(true));
            var cancelReg = ct.Register(() => cts.SetCanceled());
            return cts.Task.ContinueWith((v) =>
            {
                reg.Dispose();
                cancelReg.Dispose();
                return v;
            }, ct);
        }

        /// <summary>
        /// Returns a cancellation token with timeout.
        /// </summary>
        /// <param name="externalToken">External token to add timeout to</param>
        public static CancellationToken AddTimeout(this CancellationToken externalToken, TimeSpan timeout)
        {
            var resultSource = CancellationTokenSource.CreateLinkedTokenSource(externalToken,
                new CancellationTokenSource(timeout).Token);

            return resultSource.Token;
        }
    }
}
