using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    /// <summary>
    /// Provides functions to restart/stop a timer.
    /// </summary>
    public interface ISchedulerTimer : IDisposable
    {
        /// <summary>
        /// Stop the timer.
        /// </summary>
        void Stop();

        /// <summary>
        /// Restart the timer. If the timer is stopped than it's started again.
        /// </summary>
        void Restart();
    }

    public static class SchedulerTimerMixins
    {
        private class SchedulerTimer : ISchedulerTimer
        {
            public SchedulerTimer(Action scheduleAction)
            {
                m_scheduleAction = scheduleAction;

                // Start running upon construction.
                Restart();
            }

            public void Dispose()
            {
                if (m_isDisposed)
                    return;

                Stop();

                // Nullify to allow resources release.
                m_scheduleAction = null;

                m_isDisposed = true;
            }

            public void Stop()
            {
                if (m_isDisposed)
                    throw new ObjectDisposedException(typeof(ISchedulerTimer).Name);

                if (m_currentSchedule != null)
                    m_currentSchedule.Dispose();
                m_currentSchedule = null;
            }

            public void Restart()
            {
                if (m_isDisposed)
                    throw new ObjectDisposedException(typeof(ISchedulerTimer).Name);

                Stop();
                m_scheduleAction();
            }

            private Action m_scheduleAction;
            private bool m_isDisposed;

            private IDisposable m_currentSchedule;
        }

        /// <summary>
        /// Schedule a timer once and return an object to control the timer.
        /// </summary>
        /// <param name="scheduler">Scheduler to run the timer on</param>
        /// <param name="period">Time before timer expires</param>
        /// <param name="action">Action to perform</param>
        /// <returns></returns>
        public static ISchedulerTimer ScheduleTimer(this IScheduler scheduler, TimeSpan period, Action action)
        {
            return new SchedulerTimer(() => scheduler.Schedule(period, action));
        }
    }
}
