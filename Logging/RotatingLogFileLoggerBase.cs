using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Simple unbuffered file logger which rotates the files when they reach a certain size.
    /// </summary>
    /// <remarks>
    /// NOTE: It doesn't buffer the messages. It writes them directly to the file when log is requested. Don't use this for performance critical loops.
    /// </remarks>
    public abstract class RotatingLogFileLoggerBase : FormattedLogger, IDisposable
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

        /// <summary>
        /// Request to explicitly rotate the log file.
        /// </summary>
        protected void RotateFile()
        {
            if (m_messagesQueue != null)
                m_messagesQueue.Add(null);
            else
            {
                lock (m_writeLock)
                    m_logHelper.RotateFile();
            }
        }

        /// <summary>
        /// Implemented by derrived classes to decide when should the log rotate.
        /// </summary>
        /// <remarks>It can be called from a background thread so implementation should be thread-safe</remarks>
        protected abstract bool ShouldRotate(RotatingLogHelper helper, byte[] bytes);

        private readonly object m_writeLock = new object();

        #region Background logging

        /// <summary>
        /// Set to true to perform background logging.
        /// </summary>
        public bool IsBackground
        {
            get { return m_isBackground; }
            set
            {
                if (m_isBackground == value)
                    return;

                if (m_backgroundThread != null)
                    throw new InvalidOperationException("Can't change background mode once background thread was created");
                m_isBackground = value;
            }
        }

        private BlockingCollection<string> m_messagesQueue;
        private Thread m_backgroundThread;
        private bool m_isBackground;

        private void EnsureThread()
        {
            if (IsBackground && m_backgroundThread == null)
            {
                lock (m_writeLock)
                {
                    // Double check once inside the lock.
                    if (m_backgroundThread != null)
                        return;

                    m_messagesQueue = new BlockingCollection<string>();
                    m_backgroundThread = 
                        new Thread(ThreadProc)
                        {
                            Name = "Background logging thread",
                            IsBackground = true
                        };
                    m_backgroundThread.Start();
                }
            }
        }

        private void ThreadProc()
        {
            foreach (var msg in m_messagesQueue.GetConsumingEnumerable())
            {
                if (msg == null)
                    m_logHelper.RotateFile();
                else
                    m_logHelper.Write(msg);
            }
        }

        #endregion

        #region Implementation of ILogger

        protected override void WriteFormatted(DateTimeOffset msgTime, Splat.LogLevel logLevel, string formattedMsg)
        {
            EnsureThread();

            if (m_messagesQueue != null)
            {
                // Add To the message queue if available.
                m_messagesQueue.Add(formattedMsg);
            }
            else
            {
                // Otherwise - perform logging directly.
                try
                {
                    lock (m_writeLock)
                        m_logHelper.Write(formattedMsg);
                }
                catch (Exception)
                {
                    Debug.WriteLine("Failed to write log message '{0}'", formattedMsg);
                }
            }
        }

        #endregion

        public void Dispose()
        {
            if (m_messagesQueue != null && m_backgroundThread != null)
            {
                m_messagesQueue.CompleteAdding();
                m_backgroundThread.Join();
            }
        }
    }
}