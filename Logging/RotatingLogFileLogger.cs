using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Simple unbuffered file logger which rotates the files.
    /// </summary>
    /// <remarks>
    /// NOTE: It doesn't buffer the messages. It writes them directly to the file when log is requested. Don't use this for performance critical loops.
    /// </remarks>
    public class RotatingLogFileLogger : FormattedLogger
    {
        private const string LOG_FILE_EXT = ".log";

        /// <summary>
        /// Construct a new simple file logger.
        /// </summary>
        /// <param name="filenameBase">Directory + base name for the logs</param>
        /// <param name="maxFileSize">Max file size before rotation occurs</param>
        /// <param name="maxLogFiles">Maximum back log files to keep.</param>
        public RotatingLogFileLogger(string filenameBase, long maxFileSize = 1 * 1024 * 1024, int maxLogFiles = 10)
        {
            m_maxFileSize = maxFileSize;
            m_maxLogFiles = maxLogFiles;

            var logPath = Path.GetDirectoryName(filenameBase);
            if ( string.IsNullOrWhiteSpace(logPath) )
                throw new ArgumentException("Directory part can't be empty", "filenameBase");
            m_logPath = Directory.CreateDirectory(logPath);
            m_logFilePattern = Path.GetFileName(filenameBase);

            m_logFilename = Path.Combine(m_logPath.FullName, m_logFilePattern + LOG_FILE_EXT);

            // Initialize the filesize for bookkeeping.
            if (File.Exists(m_logFilename))
            {
                FileInfo fi = new FileInfo(m_logFilename);
                m_currentLogFileSize = fi.Length;
            }
        }

        private readonly DirectoryInfo m_logPath;
        private readonly string m_logFilePattern;

        private readonly string m_logFilename;

        private readonly long m_maxFileSize = 1 * 1024 * 1024;
        private readonly int m_maxLogFiles = 10;

        private long m_currentLogFileSize;

        private readonly object m_writeLock = new object();

        #region Implementation of ILogger

        protected override void WriteFormatted( string formattedMsg )
        {
            var msgBytes = Encoding.UTF8.GetBytes(formattedMsg);

            lock (m_writeLock)
            {
                if (m_currentLogFileSize + msgBytes.Length > m_maxFileSize)
                    RotateFile();

                using (var writer = new FileStream(m_logFilename, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    writer.Write(msgBytes, 0, msgBytes.Length);
                    m_currentLogFileSize += msgBytes.Length;
                }
            }
        }

        #endregion

        private void RotateFile()
        {
            var allLogs =
                m_logPath
                    .EnumerateFiles(m_logFilePattern + ".??" + LOG_FILE_EXT)
                    .OrderBy(f => f.Name)
                    .ToArray();
            try
            {
                // Remove the previous logs. Keep only the latest N
                foreach (var f in allLogs.Skip(m_maxLogFiles - 2))
                    f.Delete();

                string newName;
                foreach (var f in allLogs.Reverse())
                {
                    var name = f.Name;
                    var idxNumberString = name.Substring(name.Length - LOG_FILE_EXT.Length - 2, 2);
                    long index = int.Parse(idxNumberString);

                    newName = Path.Combine(
                        m_logPath.FullName,
                        m_logFilePattern + String.Format(".{0:D2}", index + 1) + LOG_FILE_EXT);
                    f.MoveTo(newName);
                }

                newName = Path.Combine(
                    m_logPath.FullName,
                    m_logFilePattern + ".01" + LOG_FILE_EXT);
                File.Move(m_logFilename, newName);
                m_currentLogFileSize = 0;
            }
            catch (Exception)
            {
                // Ignore the error and keep writing to the same file.
            }
        }
    }
}