using System;

namespace Xperitos.Common.Utils
{
    public static class UnixTime
    {
        /// <summary>
        /// Convert the given date time offset to unit time in milliseconds.
        /// </summary>
        public static long ToUnixTimeMS(this DateTimeOffset time)
        {
            return (long)(time - Epoch).TotalMilliseconds;
        }

        public static DateTimeOffset FromUnixTimeMS(long time)
        {
            return Epoch.AddMilliseconds(time);
        }

        private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}
