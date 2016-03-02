using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    public static class SemaphoreSlimMixins
    {
        /// <summary>
        /// Wait for the lock and returns a disposable to release it.
        /// e.g.:
        /// using (await sem.DoLockAsync())
        /// {
        ///     // Protected block come here!
        /// }
        /// </summary>
        public static async Task<IDisposable> LockAsync(this SemaphoreSlim sem)
        {
            await sem.WaitAsync().ConfigureAwait(false);
            return Disposable.Create(() => sem.Release());
        }

        /// <summary>
        /// Wait for the lock and returns a disposable to release it.
        /// e.g.:
        /// using (await sem.DoLockAsync())
        /// {
        ///     // Protected block come here!
        /// }
        /// </summary>
        public static async Task<IDisposable> LockAsync(this SemaphoreSlim sem, CancellationToken ct)
        {
            await sem.WaitAsync(ct).ConfigureAwait(false);
            return Disposable.Create(() => sem.Release());
        }
    }
}
