using System;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    public static class SchedulerMixins
    {
        class AsyncPeriodicScheduler : IDisposable
        {
            public AsyncPeriodicScheduler(IScheduler scheduler, TimeSpan period, Func<CancellationToken, Task> asyncTask, bool scheduleImmediately)
            {
                m_scheduler = scheduler;
                m_period = period;
                m_asyncTask = asyncTask;

                // Do initial schedule.
                if (scheduleImmediately)
                    ScheduleAction(scheduler.Now, true);
                else
                    ScheduleAction(scheduler.Now + period);
            }

            private readonly IScheduler m_scheduler;
            private readonly TimeSpan m_period;
            private readonly Func<CancellationToken, Task> m_asyncTask;

            private void ScheduleAction(DateTimeOffset dueTime, bool forceSchedule = false)
            {
                if (m_cancellationTokenSource.IsCancellationRequested)
                    return;

                if (forceSchedule)
                {
                    m_lastSchedule = m_scheduler.Schedule(() => RunAction(dueTime));
                    return;
                }

                while (dueTime < m_scheduler.Now)
                    dueTime += m_period;

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
        public static IDisposable SchedulePeriodicAsync(this IScheduler scheduler, TimeSpan period, Func<CancellationToken, Task> asyncTask, bool scheduleImmediately = false)
        {
            return new AsyncPeriodicScheduler(scheduler, period, asyncTask, scheduleImmediately);
        }
    }
}
