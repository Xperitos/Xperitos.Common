using System;

namespace Xperitos.Common.Utils
{
    public static class TimeRoundingMixins
    {
        private static long Round(this long dateTicks, TimeSpan span)
        {
            long ticks = (dateTicks + (span.Ticks / 2)) / span.Ticks;
            return ticks * span.Ticks;
        }
        private static long Round(this long dateTicks, TimeSpan span, TimeSpan grace)
        {
            long ticks = (dateTicks + (span.Ticks - grace.Ticks)) / span.Ticks;
            return ticks * span.Ticks;
        }
        private static long Floor(this long dateTicks, TimeSpan span)
        {
            long ticks = (dateTicks / span.Ticks);
            return ticks * span.Ticks;
        }
        private static long Ceil(this long dateTicks, TimeSpan span)
        {
            long ticks = (dateTicks + span.Ticks - 1) / span.Ticks;
            return ticks * span.Ticks;
        }

        /// <summary>
        /// Round date the to given timespan.
        /// </summary>
        public static DateTime Round(this DateTime date, TimeSpan span)
        {
            return new DateTime(date.Ticks.Round(span), date.Kind);
        }

        /// <summary>
        /// Round date the to given timespan.
        /// </summary>
        public static DateTime Round(this DateTime date, TimeSpan span, TimeSpan grace)
        {
            return new DateTime(date.Ticks.Round(span, grace), date.Kind);
        }

        /// <summary>
        /// Round down the date to the given timespan.
        /// </summary>
        public static DateTime Floor(this DateTime date, TimeSpan span)
        {
            return new DateTime(date.Ticks.Floor(span), date.Kind);
        }

        /// <summary>
        /// Round up the date to the given timespan.
        /// </summary>
        public static DateTime Ceil(this DateTime date, TimeSpan span)
        {
            return new DateTime(date.Ticks.Ceil(span), date.Kind);
        }


        /// <summary>
        /// Round date the to given timespan.
        /// </summary>
        public static DateTimeOffset Round(this DateTimeOffset date, TimeSpan span)
        {
            return new DateTimeOffset(date.Ticks.Round(span), date.Offset);
        }

        /// <summary>
        /// Round date the to given timespan.
        /// </summary>
        public static DateTimeOffset Round(this DateTimeOffset date, TimeSpan span, TimeSpan grace)
        {
            return new DateTimeOffset(date.Ticks.Round(span, grace), date.Offset);
        }

        /// <summary>
        /// Round down the date to the given timespan.
        /// </summary>
        public static DateTimeOffset Floor(this DateTimeOffset date, TimeSpan span)
        {
            return new DateTimeOffset(date.Ticks.Floor(span), date.Offset);
        }

        /// <summary>
        /// Round up the date to the given timespan.
        /// </summary>
        public static DateTimeOffset Ceil(this DateTimeOffset date, TimeSpan span)
        {
            return new DateTimeOffset(date.Ticks.Ceil(span), date.Offset);
        }


        /// <summary>
        /// Round timespan the to given timespan.
        /// </summary>
        public static TimeSpan Round(this TimeSpan time, TimeSpan span)
        {
            return new TimeSpan(time.Ticks.Round(span));
        }

        /// <summary>
        /// Round timespan the to given timespan.
        /// </summary>
        public static TimeSpan Round(this TimeSpan time, TimeSpan span, TimeSpan grace)
        {
            return new TimeSpan(time.Ticks.Round(span, grace));
        }

        /// <summary>
        /// Round down the date to the given timespan.
        /// </summary>
        public static TimeSpan Floor(this TimeSpan time, TimeSpan span)
        {
            return new TimeSpan(time.Ticks.Floor(span));
        }

        /// <summary>
        /// Round up the timespan to the given timespan.
        /// </summary>
        public static TimeSpan Ceil(this TimeSpan time, TimeSpan span)
        {
            return new TimeSpan(time.Ticks.Ceil(span));
        }
    }
}