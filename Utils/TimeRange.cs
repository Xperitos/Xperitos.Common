using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    /// <summary>
    /// Provide a timestamp.
    /// </summary>
    public interface ITimestamp
    {
        DateTimeOffset Timestamp { get; }
    }

	/// <summary>
	/// Provides a timestamp and a duration.
	/// </summary>
	public interface ITimeRange : ITimestamp
    {
        TimeSpan TimeRange { get; }
    }

    public static class Timestamp
    {
        /// <summary>
        /// Return the current timestamp.
        /// </summary>
        public static ITimestamp Now => new SimpleTimestamp(DateTimeOffset.Now);

	    public static ITimestamp Create(DateTimeOffset time)
        {
            return new SimpleTimestamp(time);
        }

        public static ITimeRange Create(DateTimeOffset time, TimeSpan range)
        {
            return new SimpleTimeRange(time, range);
        }
    }

    public class SimpleTimestamp : ITimestamp
    {
        public SimpleTimestamp(DateTimeOffset timestamp)
        {
            Timestamp = timestamp;
        }

        #region Implementation of ITimestamp

        public DateTimeOffset Timestamp { get; private set; }

        #endregion
    }

    public class SimpleTimeRange : ITimeRange
    {
        public SimpleTimeRange(DateTimeOffset timestamp, TimeSpan timeRange)
        {
            TimeRange = timeRange;
            Timestamp = timestamp;
        }

        #region Implementation of ITimestamp

        public DateTimeOffset Timestamp { get; private set; }

        #endregion

        #region Implementation of ITimeRange

        public TimeSpan TimeRange { get; private set; }

        #endregion
    }

    public class PrioritizedTimeRange : SimpleTimeRange
    {
        public PrioritizedTimeRange(DateTimeOffset timestamp, TimeSpan timeRange, bool priority) : base(timestamp, timeRange)
        {
            Priority = priority;
        }
        
        public bool Priority { get; private set; }        
    }

    public static class TimestampMixins
    {
        public static DateTimeOffset GetStartTime(this ITimestamp item)
        {
            return item.Timestamp;
        }

        public static DateTimeOffset GetEndTime(this ITimeRange item)
        {
            return item.Timestamp + item.TimeRange;
        }

        public static DateTimeOffset GetCenterTime(this ITimeRange item)
        {
            return item.Timestamp.AddTicks(item.TimeRange.Ticks / 2);
        }

        public static ITimeRange OffsetBy(this ITimeRange range, TimeSpan offset)
        {
            return Timestamp.Create(range.Timestamp - offset, range.TimeRange);
        }

        /// <summary>
        /// Inflates the given time stamps into time ranges using the given before/after parameters.
        /// </summary>
        public static ITimeRange InflateBy(this ITimestamp timestamp, TimeSpan before, TimeSpan after)
        {
            return new SimpleTimeRange(timestamp.Timestamp - before, before + after);
        }

        /// <summary>
        /// Inflates the given time stamps into time ranges using the given before/after parameters.
        /// </summary>
        public static IEnumerable<ITimeRange> InflateBy(this IEnumerable<ITimestamp> timestamps, TimeSpan before, TimeSpan after)
        {
            return timestamps.Select(v => v.InflateBy(before, after));
        }

        /// <summary>
        /// Merges Overlapping timeranges.
        /// </summary>
        public static IEnumerable<ITimeRange> MergeOverlapping(this IEnumerable<ITimeRange> ranges)
        {
            // Ensure the ranges are sorted.
            var orderedRanges = ranges.OrderBy(range => range.Timestamp);

            DateTimeOffset? startTime = null;
            DateTimeOffset? endTime = null;
            foreach (var range in orderedRanges)
            {
                if (startTime.HasValue)
                {
                    if (endTime > range.GetStartTime())
                    {
                        // They overlap - join to the current range and move on to the next item.
                        if (endTime < range.GetEndTime())
                            endTime = range.GetEndTime();
                        continue;
                    }

                    // Return the previous range.
                    yield return new SimpleTimeRange(startTime.Value, endTime.Value - startTime.Value);
                }

                // Initialize a new range.
                startTime = range.GetStartTime();
                endTime = range.GetEndTime();
            }

            // Return the remaining range.
            if (startTime.HasValue)
                yield return new SimpleTimeRange(startTime.Value, endTime.Value - startTime.Value);
        }

        /// <summary>
        /// Merges Overlapping timeranges regarding to thier priority (high/low)
        /// </summary>
        /// <param name="ranges"></param>
        public static IEnumerable<PrioritizedTimeRange> PrioritzedMergeOverlapping(this IEnumerable<PrioritizedTimeRange> ranges)
        {
            // Ensure the ranges are sorted.            
            var higherPriority = ranges.Where(range => range.Priority).Select(v => new SimpleTimeRange(v.Timestamp, v.TimeRange)).MergeOverlapping().ToList();
            var lowerPriority = ranges.Where(range => !range.Priority).Select(v => new SimpleTimeRange(v.Timestamp, v.TimeRange)).MergeOverlapping().ToList();

            var allRanges = higherPriority.Select(v => new PrioritizedTimeRange(v.Timestamp, v.TimeRange,true)).ToList();
            allRanges.AddRange(lowerPriority.Select(v => new PrioritizedTimeRange(v.Timestamp, v.TimeRange, false)).ToList());
            allRanges = allRanges.OrderBy(range => range.Timestamp).ThenByDescending(range => range.Priority).ToList();

            DateTimeOffset? startTime = null;
            DateTimeOffset? endTime = null;
            DateTimeOffset? tempStartTime = null;
            DateTimeOffset? tempEndTime = null;
            var currPriority = false;
            var tempPriority = false;

            foreach (var range in allRanges)
            {
                if (startTime.HasValue)
                {
                    if (endTime > range.GetStartTime())
                    {
                        // new range has lower priority
                        if (currPriority && !range.Priority)
                        {
                            if (endTime >= range.GetEndTime())
                            {
                                continue;
                            }
                            tempStartTime = startTime.Value;
                            tempEndTime = endTime.Value;
                            tempPriority = currPriority;
                            startTime = tempEndTime;
                            endTime = range.GetEndTime();
                            currPriority = range.Priority;

                            yield return new PrioritizedTimeRange(tempStartTime.Value, tempEndTime.Value - tempStartTime.Value, tempPriority);
                            continue;
                        }
                        // new range has higher priority
                        else if (!currPriority && range.Priority)
                        {
                            if (endTime <= range.GetEndTime())
                            {
                                tempStartTime = startTime.Value;
                                tempEndTime = range.GetStartTime();
                                tempPriority = currPriority;                                
                                yield return new PrioritizedTimeRange(tempStartTime.Value, tempEndTime.Value - tempStartTime.Value, tempPriority);
                            }else
                            {
                                tempStartTime = startTime.Value;
                                tempEndTime = range.GetStartTime();
                                tempPriority = currPriority;
                                yield return new PrioritizedTimeRange(tempStartTime.Value, tempEndTime.Value - tempStartTime.Value, tempPriority);
                                yield return new PrioritizedTimeRange(range.GetStartTime(), range.GetEndTime() - range.GetStartTime(), range.Priority);

                                startTime = range.GetEndTime();
                                continue;                                                                                                
                            }
                        }
                        else
                        {
                            if (endTime < range.GetEndTime())
                            {
                                endTime = range.GetEndTime();
                            }                        
                            continue;
                        }

                    }

                    yield return new PrioritizedTimeRange(startTime.Value, endTime.Value - startTime.Value, currPriority);
                }

                startTime = range.GetStartTime();
                endTime = range.GetEndTime();
                currPriority = range.Priority;
            }

            // Return the remaining range.
            if (startTime.HasValue)
                yield return new PrioritizedTimeRange(startTime.Value, endTime.Value - startTime.Value, currPriority);            
        }

        /// <summary>
        /// Break big time ranges into smaller consecutive blocks.
        /// </summary>
        public static IEnumerable<ITimeRange> BreakBigBlocks(this IEnumerable<ITimeRange> ranges, TimeSpan bigBlockLength)
        {
            foreach (var range in ranges)
            {
                if (range.TimeRange < bigBlockLength)
                    yield return range;
                else
                {
                    var startTime = range.GetStartTime();
                    var endTime = range.GetEndTime();
                    while (startTime < endTime)
                    {
                        var duration = bigBlockLength;
                        if (duration > endTime - startTime)
                            duration = endTime - startTime;
                        yield return Timestamp.Create(startTime, duration);
                        startTime = startTime + duration;
                    }
                }
            }
        }
    }
}
