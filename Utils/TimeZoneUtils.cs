﻿using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    /// <summary>
    /// Convert time Windows TimeZoneInfo to/from IANA format.
    /// Implementation was based on http://stackoverflow.com/questions/17348807/how-to-translate-between-windows-and-iana-time-zones
    /// </summary>
    public static class TimeZoneUtils
    {
        /// <summary>
        /// Convert Iana time zone string representation to TimeZoneInfo
        /// </summary>
        /// <param name="source"></param>
        /// <param name="ianaZoneId"></param>
        /// <returns></returns>
        public static TimeZoneInfo FromIana(string ianaZoneId)
        {
            var utcZones = new[] { "Etc/UTC", "Etc/UCT", "Etc/GMT" };
            if (utcZones.Contains(ianaZoneId, StringComparer.Ordinal))
            {
                return TimeZoneInfo.FindSystemTimeZoneById("UTC");
            }
                

            var tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;

            // resolve any link, since the CLDR doesn't necessarily use canonical IDs
            var links = tzdbSource.CanonicalIdMap
                .Where(x => x.Value.Equals(ianaZoneId, StringComparison.Ordinal))
                .Select(x => x.Key);

            // resolve canonical zones as well
            var possibleZones = tzdbSource.CanonicalIdMap.ContainsKey(ianaZoneId)
                ? links.Concat(new[] { tzdbSource.CanonicalIdMap[ianaZoneId] })
                : links;

            if (OSDetector.CurrentOS == OSDetector.OSFamily.Windows)
            {
                // map the windows zone
                var mappings = tzdbSource.WindowsMapping.MapZones;
                var item = mappings.FirstOrDefault(x => x.TzdbIds.Any(possibleZones.Contains));
                if (item == null)
                    return null;

                return TimeZoneInfo.FindSystemTimeZoneById(item.WindowsId);
            } 
            else
            {
                return possibleZones.Select(id =>
                {
                    try
                    {
                        return TimeZoneInfo.FindSystemTimeZoneById(id);
                    }
                    catch (TimeZoneNotFoundException)
                    {
                        return null;
                    }
                })
                .FirstOrDefault(tz => tz != null);
            }
        }

        /// <summary>
        /// Convert TimeZoneInfo to IANA format
        /// </summary>
        /// <param name="source"></param>
        public static string ToIana(this TimeZoneInfo source)
        {
            string windowsZoneId = source.Id;

            if (windowsZoneId.Equals("UTC", StringComparison.Ordinal))
                return "Etc/UTC";

            var tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(windowsZoneId);
            if (tzi == null) return null;
            var tzid = tzdbSource.WindowsMapping.PrimaryMapping.GetValueOrDefault(tzi.Id);
            if (tzid == null) return null;
            return tzdbSource.CanonicalIdMap[tzid];
        }
        public static DateTimeOffset ParseExactWithTimeZone(string input, string format, TimeZoneInfo timezone)
        {
            var parsedDateLocal = DateTimeOffset.ParseExact(input, format, System.Globalization.CultureInfo.InvariantCulture);
            var tzOffset = timezone.GetUtcOffset(parsedDateLocal.DateTime);
            var parsedDateTimeZone = new DateTimeOffset(parsedDateLocal.DateTime, tzOffset);
            return parsedDateTimeZone;
        }

        public static DateTime ConvertToLocal(this TimeZoneInfo tzi, DateTime time)
        {
            if (time.Kind == DateTimeKind.Local)
                throw new ArgumentException("Kind property of dateTime is Local");

            if (tzi == TimeZoneInfo.Utc)
                return DateTime.SpecifyKind(time, DateTimeKind.Utc);

            var utcOffset = tzi.GetUtcOffset(time);

            var kind = (tzi == TimeZoneInfo.Local) ? DateTimeKind.Local : DateTimeKind.Unspecified;

            return new DateTime(time.Ticks + utcOffset.Ticks, kind);
        }

        public static DateTimeOffset ConvertToLocal(this TimeZoneInfo tzi, DateTimeOffset time)
        {
            return TimeZoneInfo.ConvertTime(time, tzi);
        }

        public static DateTime ConvertToUTC(this TimeZoneInfo tzi, DateTime time)
        {
            if (time.Kind == DateTimeKind.Utc)
                return time;

            var utcOffset = tzi.GetUtcOffset(time);

            return new DateTime(time.Ticks - utcOffset.Ticks, DateTimeKind.Utc);
        }

		/// <summary>
		/// Get a 'date time offset', check if it is an ambigous one (means that it falls in the hour of change from PDT to PST)
		/// If it is the case, use the recorded offset in order to convert this date to UTC
		/// The recorded offset is the offset recorded on the execution time of the code and it "knows" at the moment
		/// Of the record if 'now' we are before or after the change from PDT to PST
		/// </summary>
		/// <param name="dateTimeOffset"></param>
		/// <param name="recordedOffset"></param>
		/// <param name="timeZoneInfo"></param>
		/// <returns></returns>
		public static DateTimeOffset FixIfAmbiguous(this DateTimeOffset dateTimeOffset, TimeSpan recordedOffset, TimeZoneInfo timeZoneInfo)
		{
			// if it's not ambigous, nothing to do with it.
			if (!timeZoneInfo.IsAmbiguousTime(dateTimeOffset))
				return dateTimeOffset;

			// if the recorded offset is equal to the dateTimeOffset, nothing to do.
			if (TimeSpan.Equals(dateTimeOffset.Offset, recordedOffset))
			{
				Log.Verbose("Ambiguous time: {ambiguousTime} offset: {ambiguousTimeOffset} is equal to recorded offset: {offset}, probably running after the PDT to PST change, nothing to do.", dateTimeOffset.LocalDateTime.ToString(), dateTimeOffset.Offset.ToString(), recordedOffset.ToString());
				return dateTimeOffset;
			}

			// log the required fix
			Log.Verbose("Fixing ambiguous time: {ambiguousTime}, with offset: {offSet}, while recordedOffset is: {recordedOffset}", dateTimeOffset.LocalDateTime.ToString(), dateTimeOffset.Offset.ToString(), recordedOffset.ToString());

			// fix the ambiguous time
			var fixedAmbiguousTime = new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, dateTimeOffset.Minute, dateTimeOffset.Second, recordedOffset);

			// log the fixed result
			Log.Verbose("Fixed ambiguous time: {fixedAmbiguousTime}, to use offset: {offset}", fixedAmbiguousTime.LocalDateTime.ToString(), fixedAmbiguousTime.Offset.ToString());

			// return result
			return fixedAmbiguousTime;
		}
	}
}
