using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Simple unbuffered file logger which rotates the files when they reach a certain size.
    /// </summary>
    /// <remarks>
    /// NOTE: It doesn't buffer the messages. It writes them directly to the file when log is requested. Don't use this for performance critical loops.
    /// </remarks>
    public abstract class RotatingLogFileLoggerBase : FormattedLogger
    {
        private const string LOG_FILE_EXT = ".log";

        /// <summary>
        /// Construct a new simple file logger.
        /// </summary>
        /// <param name="filenameBase">Directory + base name for the logs</param>
        /// <param name="maxLogFiles">Maximum back log files to keep.</param>
        protected RotatingLogFileLoggerBase(string filenameBase, int maxLogFiles = 10)
        {
            var logPath = Path.GetDirectoryName(filenameBase);
            if ( string.IsNullOrWhiteSpace(logPath) )
                throw new ArgumentException("Directory part can't be empty", "filenameBase");

            m_logHelper = new RotatingLogHelper(
                logPath,
                Path.GetFileName(filenameBase),
                LOG_FILE_EXT,
                maxLogFiles,
                ShouldRotate);
        }

        private readonly RotatingLogHelper m_logHelper;

        protected RotatingLogHelper LogHelper { get { return m_logHelper; } }

        /// <summary>
        /// Implemented by derrived classes to decide when should the log rotate.
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        protected abstract bool ShouldRotate(RotatingLogHelper helper);

        private readonly object m_writeLock = new object();

        #region Implementation of ILogger

        protected override void WriteFormatted(DateTimeOffset msgTime, Splat.LogLevel logLevel, string formattedMsg)
        {
            lock (m_writeLock)
                m_logHelper.Write(formattedMsg);
        }

        #endregion
    }

    public class RotatingLogFileLogger : RotatingLogFileLoggerBase
    {
        /// <summary>
        /// Construct a new simple file logger.
        /// </summary>
        /// <param name="filenameBase">Directory + base name for the logs</param>
        /// <param name="maxFileSize">Max file size before rotation occurs</param>
        /// <param name="maxLogFiles">Maximum back log files to keep.</param>
        public RotatingLogFileLogger(string filenameBase, long maxFileSize = 1 * 1024 * 1024, int maxLogFiles = 10)
            : base(filenameBase, maxLogFiles)
        {
            m_maxFileSize = maxFileSize;
        }

        private readonly long m_maxFileSize;

        protected override bool ShouldRotate(RotatingLogHelper helper)
        {
            return helper.CurrentLogFileSize > m_maxFileSize;
        }
    }
}
