using System;

namespace Xperitos.Common.Utils
{
    public static class StartOfDateMixins
    {
        public static DateTime StartOfDay(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, dt.Kind);
        }

        /// <summary>
        /// Returns the start of the week for the specified date (according to the startOfWeek value).
        /// </summary>
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).StartOfDay();
        }

        public static DateTime StartOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, dt.Kind);
        }

        public static DateTime StartOfYear(this DateTime dt)
        {
            return new DateTime(dt.Year, 1, 1, 0, 0, 0, dt.Kind);
        }


        public static DateTimeOffset StartOfDay(this DateTimeOffset dt)
        {
            return new DateTimeOffset(dt.Year, dt.Month, dt.Day, 0, 0, 0, dt.Offset);
        }

        /// <summary>
        /// Returns the start of the week for the specified date (according to the startOfWeek value).
        /// </summary>
        public static DateTimeOffset StartOfWeek(this DateTimeOffset dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).StartOfDay();
        }

        public static DateTimeOffset StartOfMonth(this DateTimeOffset dt)
        {
            return new DateTimeOffset(dt.Year, dt.Month, 1, 0, 0, 0, dt.Offset);
        }

        public static DateTimeOffset StartOfYear(this DateTimeOffset dt)
        {
            return new DateTimeOffset(dt.Year, 1, 1, 0, 0, 0, dt.Offset);
        }
    }
}