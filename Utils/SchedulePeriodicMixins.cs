using System;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    [Flags]
    public enum SchedulePeriodicOptions
    {
        /// <summary>
        /// If set then the task will execute immediately.
        /// </summary>
        ExecuteNow = 1,

        /// <summary>
        /// When a task completes execution and it exceeds the time between executions, 
        /// if set, it will run ASAP; if not set it will be scheduled in the next time it SHOULD run.
        /// </summary>
        RescheduleAsSoonAsPossible = 2,

        /// <summary>
        /// When specified, an initial delay will be added to the current time to make sure the timer occurs on ROUND intervals (e.g. interval of 12 hours will occur at 12:00 and 00:00 ONLY).
        /// </summary>
        UseRoundIntervals = 4
    }

    public static class SchedulePeriodicMixins
    {
        class AsyncPeriodicScheduler : IDisposable
        {
            public AsyncPeriodicScheduler(IScheduler scheduler, TimeSpan initialDelay, TimeSpan period, Func<CancellationToken, Task> asyncTask, bool rescheduleAsSoonAsPossible)
            {
                Initialize(scheduler, scheduler.Now, initialDelay, period, asyncTask, false, rescheduleAsSoonAsPossible, true);
            }

            public AsyncPeriodicScheduler(IScheduler scheduler, TimeSpan period, Func<CancellationToken, Task> asyncTask, SchedulePeriodicOptions options)
            {
                TimeSpan initialDelay = TimeSpan.Zero;
                var now = scheduler.Now;

                if (options.HasFlag(SchedulePeriodicOptions.UseRoundIntervals))
                    initialDelay = (now.Floor(period) - now);

                Initialize(
                    scheduler, 
                    now, 
                    initialDelay, 
                    period, 
                    asyncTask, 
                    options.HasFlag(SchedulePeriodicOptions.ExecuteNow), 
                    options.HasFlag(SchedulePeriodicOptions.RescheduleAsSoonAsPossible),
                    false);
            }

            private void Initialize(IScheduler scheduler, DateTimeOffset now, TimeSpan initialDelay, TimeSpan period, Func<CancellationToken, Task> asyncTask,
                bool forceSchedule, bool rescheduleAsSoonAsPossible, bool scheduleInDelay)
            {
                m_scheduler = scheduler;
                m_period = period;
                m_asyncTask = asyncTask;
                m_rescheduleAsSoonAsPossible = rescheduleAsSoonAsPossible;

                // Do initial schedule.
                if (forceSchedule)
                    ScheduleAction(now + initialDelay, true);
                else if (!scheduleInDelay)
                    ScheduleAction(now + period + initialDelay);
                else
                    ScheduleAction(now + initialDelay);
            }

            private IScheduler m_scheduler;
            private TimeSpan m_period;
            private Func<CancellationToken, Task> m_asyncTask;
            private bool m_rescheduleAsSoonAsPossible;

            private void ScheduleAction(DateTimeOffset dueTime, bool forceSchedule = false)
            {
                if (m_cancellationTokenSource.IsCancellationRequested)
                    return;

                if (forceSchedule)
                {
                    m_lastSchedule = m_scheduler.Schedule(() => RunAction(dueTime));
                    return;
                }

                if (dueTime < m_scheduler.Now)
                {
                    if (m_rescheduleAsSoonAsPossible)
                        dueTime = m_scheduler.Now;
                    else
                    {
                        while (dueTime < m_scheduler.Now)
                            dueTime += m_period;
                    }
                }

                m_lastSchedule = m_scheduler.Schedule(dueTime, () => RunAction(dueTime));
            }

            private async void RunAction(DateTimeOffset dueTime)
            {
                if (m_cancellationTokenSource.IsCancellationRequested)
                    return;

                try
                {
                    await m_asyncTask(m_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancelled task.
                }

                ScheduleAction(dueTime + m_period);
            }

            private IDisposable m_lastSchedule;

            #region Implementation of IDisposable

            readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();

            public void Dispose()
            {
                if ( m_cancellationTokenSource.IsCancellationRequested )
                    return;

                m_cancellationTokenSource.Cancel();
                if (m_lastSchedule != null)
                    m_lastSchedule.Dispose();

                m_lastSchedule = null;
            }

            #endregion
        }

        /// <summary>
        /// Perform a periodic timer for an async event. The next timer will not schedule until the task completes.
        /// This overload accepts an initial delay before running the timer and then a periodic time for each interval.
        /// </summary>
        public static IDisposable SchedulePeriodicAsync(this IScheduler scheduler, TimeSpan initialDelay, TimeSpan period, Func<CancellationToken, Task> asyncTask, bool rescheduleAsSoonAsPossible = false)
        {
            return new AsyncPeriodicScheduler(scheduler, initialDelay, period, asyncTask, rescheduleAsSoonAsPossible);
        }

        /// <summary>
        /// Perform a periodic timer for an async event. The next timer will not schedule until the task completes.
        /// </summary>
        public static IDisposable SchedulePeriodicAsync(this IScheduler scheduler, TimeSpan period, Func<CancellationToken, Task> asyncTask, SchedulePeriodicOptions options)
        {
            return new AsyncPeriodicScheduler(scheduler, period, asyncTask, options);
        }

        /// <summary>
        /// Perform a periodic timer for an async event. The next timer will not schedule until the task completes.
        /// </summary>
        public static IDisposable SchedulePeriodicAsync(this IScheduler scheduler, TimeSpan period, Func<CancellationToken, Task> asyncTask, bool scheduleImmediately = false)
        {
            return SchedulePeriodicAsync(scheduler, period, asyncTask, scheduleImmediately ? SchedulePeriodicOptions.ExecuteNow : 0);
        }
    }
}
