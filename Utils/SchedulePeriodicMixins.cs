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
        RescheduleAsSoonAsPossible = 2
    }

    public static class SchedulePeriodicMixins
    {
        class AsyncPeriodicScheduler : IDisposable
        {
            public AsyncPeriodicScheduler(IScheduler scheduler, TimeSpan period, Func<CancellationToken, Task> asyncTask, SchedulePeriodicOptions options)
            {
                m_scheduler = scheduler;
                m_period = period;
                m_asyncTask = asyncTask;
                m_options = options;

                // Do initial schedule.
                if (options.HasFlag(SchedulePeriodicOptions.ExecuteNow))
                    ScheduleAction(scheduler.Now, true);
                else
                    ScheduleAction(scheduler.Now + period);
            }

            private readonly IScheduler m_scheduler;
            private readonly TimeSpan m_period;
            private readonly Func<CancellationToken, Task> m_asyncTask;
            private readonly SchedulePeriodicOptions m_options;

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
                    if ( m_options.HasFlag(SchedulePeriodicOptions.RescheduleAsSoonAsPossible) )
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
