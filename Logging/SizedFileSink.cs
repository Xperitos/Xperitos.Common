using System;
using System.IO;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// File sink that provides an estimated length wrote so far.
    /// </summary>
    sealed class SizedFileSink : IDisposable, ILogEventSink
    {
        public SizedFileSink(string path, ITextFormatter textFormatter, Encoding encoding)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (textFormatter == null) throw new ArgumentNullException(nameof(textFormatter));

            m_textFormatter = textFormatter;

            TryCreateDirectory(path);

            var file = File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            var writer = new StreamWriter(file, encoding ?? Encoding.UTF8);
            m_output = new CountingTextWriter(writer);
            m_initialFileSize = file.Length;
        }

        private readonly long m_initialFileSize;
        readonly CountingTextWriter m_output;
        readonly ITextFormatter m_textFormatter;
        readonly object m_syncRoot = new object();

        /// <summary>
        /// Return the current estimated file length.
        /// </summary>
        public long EstimatedLength => m_output.WroteChars + m_initialFileSize;

        static void TryCreateDirectory(string path)
        {
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
            catch (Exception)
            {
                // Where should this be logged!?
            }
        }

        public void Dispose()
        {
            m_output.Dispose();
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            lock (m_syncRoot)
            {
                m_textFormatter.Format(logEvent, m_output);
                m_output.Flush();
            }
        }
    }
}