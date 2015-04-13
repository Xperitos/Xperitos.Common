using System;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Rotate the log file on a daily basis.
    /// </summary>
    public class DailyLogFileLogger : RotatingLogFileLoggerBase
    {
        /// <summary>
        /// Construct a new simple file logger - rotates when certain file size is reached.
        /// </summary>
        /// <param name="filenameBase">Directory + base name for the logs</param>
        /// <param name="maxLogFiles">Maximum back log files to keep.</param>
        public DailyLogFileLogger(string filenameBase, int maxLogFiles = 10)
            : base(filenameBase, maxLogFiles)
        {
        }

        protected override bool ShouldRotate(RotatingLogHelper helper, byte[] bytes)
        {
            return DateTimeOffset.UtcNow.Date != helper.CurrentLogFileCreationTime.Date;
        }
    }
}
