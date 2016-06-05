using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.IOFile;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Mimics the original RollingFileSink but adds the ability to rotate the log implicitly and upon startup.
    /// </summary>
    public sealed class RollingFileSink : ILogEventSink, IDisposable
    {
        public static readonly string DateFormat = "yyyyMMdd";
        public static readonly string DatePlaceHolder = "{date}";

        /// <summary>Construct a <see cref="RollingFileSink"/>.</summary>
        /// <param name="pathFormat">String describing the location of the log files,
        /// with {Date} in the place of the file date. E.g. "Logs\myapp-{date}.log" will result in log
        /// files such as "Logs\myapp-2013-10-20.log", "Logs\myapp-2013-10-21.log" and so on.</param>
        /// <param name="textFormatter">Formatter used to convert log events to text.</param>
        /// <param name="fileSizeLimitBytes">The maximum size, in bytes, to which a log file will be allowed to grow.
        /// For unrestricted growth, pass null. The default is 100MB.</param>
        /// <param name="retainedFileCountLimit">The maximum number of log files that will be retained,
        /// including the current log file. For unlimited retention, pass null. The default is 60.</param>
        /// <param name="encoding">Character encoding used to write the text file. The default is UTF-8.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        /// <remarks>The file will be written using the UTF-8 character set.</remarks>
        public RollingFileSink(string pathFormat,
                              ITextFormatter textFormatter,
                              long? fileSizeLimitBytes,
                              int? retainedFileCountLimit,
                              Encoding encoding = null)
        {
            if (pathFormat == null) throw new ArgumentNullException(nameof(pathFormat));
            if (fileSizeLimitBytes.HasValue && fileSizeLimitBytes < 0) throw new ArgumentException("Negative value provided; file size limit must be non-negative", nameof(fileSizeLimitBytes));
            if (retainedFileCountLimit.HasValue && retainedFileCountLimit < 1) throw new ArgumentException("Zero or negative value provided; retained file count limit must be at least 1", nameof(retainedFileCountLimit));

            pathFormat = Path.GetFullPath(pathFormat);
            var pathFormatDir = Path.GetDirectoryName(pathFormat);
            var pathFormatName = Path.GetFileName(pathFormat);
            var pathFormatDateStart = pathFormatName.ToLower().IndexOf(DatePlaceHolder);
            if (pathFormatDateStart == -1) throw new ArgumentException("Path format doesn't contain a date part", nameof(pathFormat));

            m_pathFormatPrefix = Path.Combine(pathFormatDir, pathFormatName.Substring(0, pathFormatDateStart));
            m_pathFormatPostfix = pathFormatName.Substring(pathFormatDateStart + 6);
            m_pathFormat = m_pathFormatPrefix + DatePlaceHolder + m_pathFormatPostfix;

            m_textFormatter = textFormatter;
            m_fileSizeLimitBytes = fileSizeLimitBytes;
            m_retainedFileCountLimit = retainedFileCountLimit;
            m_encoding = encoding ?? Encoding.UTF8;
        }

        private readonly ITextFormatter m_textFormatter;
        private readonly long? m_fileSizeLimitBytes;
        private readonly int? m_retainedFileCountLimit;
        private readonly Encoding m_encoding;
        private readonly bool m_buffered;
        private readonly string m_pathFormat;
        private readonly string m_pathFormatPrefix;
        private readonly string m_pathFormatPostfix;

        class LogFileName
        {
            public LogFileName(string path, string name, DateTime date, int seq)
            {
                Path = path;
                Name = name;
                Date = date;
                SequenceNumber = seq;
            }

            public string Name { get; }
            public string Path { get; }
            public int SequenceNumber { get; }
            public DateTime Date { get; }

            public string FullName => System.IO.Path.Combine(Path, Name);
        }

        private LogFileName ParseLogFileName(string fullName)
        {
            try
            {
                var path = Path.GetDirectoryName(fullName);
                var name = Path.GetFileName(fullName);

                var data = fullName.Substring(
                    m_pathFormatPrefix.Length, 
                    fullName.Length - m_pathFormatPrefix.Length - m_pathFormatPostfix.Length);

                var parts = data.Split('_');
                DateTime time = DateTime.ParseExact(parts[0], DateFormat, null);
                var seq = parts.Length > 1 ? int.Parse(parts[1]) : 0;

                return new LogFileName(path, name, time, seq);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string ConstructLogFilename(DateTime now, int sequenceNumber)
        {
            return m_pathFormatPrefix + now.ToString(DateFormat) + "_" + sequenceNumber.ToString("D2") + m_pathFormatPostfix;
        }

        /// <summary>
        /// Enumerates the log files sorted by their time and sequence number in desscending order.
        /// </summary>
        private IEnumerable<LogFileName> EnumerateFiles()
        {
            var dir = Path.GetDirectoryName(m_pathFormat);
            var name = Path.GetFileName(m_pathFormat);

            try
            {
                return Directory
                    .EnumerateFiles(dir, name.Replace(DatePlaceHolder, "*"), SearchOption.TopDirectoryOnly)
                    .Select(ParseLogFileName)
                    .Where(v => v != null)
                    .OrderByDescending(v => v.Date)
                    .ThenByDescending(v => v.SequenceNumber);
            }
            catch (Exception)
            {
                return Enumerable.Empty<LogFileName>();
            }
        }

        void ApplyRetentionPolicy(string skipFile)
        {
            if (m_retainedFileCountLimit == null)
                return;

            var filesToRemove = EnumerateFiles()
                .Skip(m_retainedFileCountLimit.Value - 1)
                .Where(v => String.Compare(v.FullName, skipFile, StringComparison.CurrentCultureIgnoreCase) != 0)
                .ToList();

            foreach (var file in filesToRemove)
            {
                try
                {
                    File.Delete(file.FullName);
                }
                catch (Exception)
                {
                    // Where should this be logged!?
                }
            }
        }

        private SizedFileSink m_currentFile;
        private DateTime? m_nextCheckpoint;
        private bool m_isDisposed;
        private readonly object m_syncRoot = new object();

        void OpenFileIfNeeded(DateTime now)
        {
            // Open file only if nothing was opened or if we reached a checkpoint to open a file.
            if (m_currentFile == null || m_nextCheckpoint <= now)
            {
                CloseFile();
                OpenFile(now);
            }
        }

        void OpenFile(DateTime now)
        {
            var date = now.Date;

            var latestFileForNow = EnumerateFiles().FirstOrDefault(v => v.Date == date);
            var sequence = (latestFileForNow?.SequenceNumber ?? 0) + 1;

            // Re-open the file on the next day.
            m_nextCheckpoint = date.AddDays(1);

            const int maxAttempts = 3;
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var newFileName = ConstructLogFilename(now, sequence);

                try
                {
                    m_currentFile = new SizedFileSink(newFileName, m_textFormatter, m_encoding);
                }
                catch (IOException ex)
                {
                    var errorCode = Marshal.GetHRForException(ex) & ((1 << 16) - 1);
                    if (errorCode == 32 || errorCode == 33)
                    {
                        // "Rolling file target {0} was locked, attempting to open next in sequence (attempt {1})", newFileName, attempt + 1);
                        sequence++;
                        continue;
                    }

                    throw;
                }

                // Purge old files.
                ApplyRetentionPolicy(newFileName);
                return;
            }
        }

        void CloseFile()
        {
            if (m_currentFile != null)
            {
                m_currentFile.Dispose();
                m_currentFile = null;
            }

            m_nextCheckpoint = null;
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            lock (m_syncRoot)
            {
                if (m_isDisposed)
                    throw new ObjectDisposedException("Object disposed");

                // Close previous file if size reached.
                if (m_currentFile != null && m_fileSizeLimitBytes.HasValue &&
                    m_currentFile.EstimatedLength >= m_fileSizeLimitBytes.Value)
                {
                    CloseFile();
                }

                OpenFileIfNeeded(DateTime.Now);

                // If the file was unable to be opened on the last attempt, it will remain
                // null until the next checkpoint passes, at which time another attempt will be made to
                // open it.
                m_currentFile?.Emit(logEvent);
            }
        }

        public void Dispose()
        {
            lock (m_syncRoot)
            {
                CloseFile();
                m_isDisposed = true;
            }
        }
    }
}
