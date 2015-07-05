using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xperitos.Common.Streams
{
    public static class NetworkStreamMixins
    {
        /// <summary>
        /// Implements a read async that supports cancellation token properly (using timeouts).
        /// </summary>
        public static Task<int> CancellableReadAsync(this NetworkStream stream, byte[] buffer, int offset, int count, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        stream.ReadTimeout = 1000;
                        return stream.Read(buffer, offset, count);
                    }
                    catch (IOException e)
                    {
                        var socketEx = e.InnerException as SocketException;

                        // If timedout - then just repeat the loop.
                        if (socketEx != null && socketEx.SocketErrorCode == SocketError.TimedOut)
                            continue;

                        throw;
                    }
                }
            }, ct);
        }

    }
}
