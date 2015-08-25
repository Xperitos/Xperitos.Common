using System;

namespace Xperitos.Common.Utils
{
    public static class TimeSpanMixins
    {
        public static TimeSpan Divide(this TimeSpan ts, double value)
        {
            return new TimeSpan((long)(ts.Ticks / value));
        }

        public static TimeSpan Multiply(this TimeSpan ts, double value)
        {
            return new TimeSpan((long)(ts.Ticks * value));
        }
    }
}