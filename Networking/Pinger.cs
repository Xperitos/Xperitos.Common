using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Xperitos.Common.Networking
{
    public static class Pinger
    {
        /// <summary>
        /// Perform a ping to the designated host.
        /// </summary>
        /// <returns>true if host was reachable</returns>
        public static Task<bool> PingAsync(string host, int echosToSend = 4, bool failOnFirst = false)
        {
            return Task.Run(() =>
            {
                try
                {
                    int timeout = 200;
                    Ping pingSender = new Ping();

                    bool hadSuccess = false;
                    for (int i = 0; i < echosToSend; i++)
                    {
                        PingReply reply = pingSender.Send(host, timeout);

                        // If one echo fails then ping fails.
                        if (reply?.Status == IPStatus.Success)
                            hadSuccess = true;
                        else
                        {
                            if (failOnFirst)
                                return false;
                        }
                    }

                    return hadSuccess;
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Returns an average ping time.
        /// </summary>
        public static Task<TimeSpan?> PingTimeAverageAsync(string host, int echoNum)
        {
            return Task.Run<TimeSpan?>(() =>
            {
                try
                {
                    long totalTime = 0;
                    int timeout = 120;
                    Ping pingSender = new Ping();

                    for (int i = 0; i < echoNum; i++)
                    {
                        PingReply reply = pingSender.Send(host, timeout);
                        if (reply.Status == IPStatus.Success)
                            totalTime += reply.RoundtripTime;
                    }

                    if (totalTime == 0)
                        return null;

                    return TimeSpan.FromMilliseconds(totalTime / (double)echoNum);
                }
                catch (Exception)
                {
                    return null;
                }
            });
        }
    }
}
