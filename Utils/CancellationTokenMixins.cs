using System;
using System.Threading;

namespace Xperitos.Common.Utils
{
    public static class CancellationTokenMixins
    {
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
