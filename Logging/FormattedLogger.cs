using System;
using System.Diagnostics;
using System.Text;
using Splat;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Base class for logger with fomatted message.
    /// </summary>
    public abstract class FormattedLogger : ILogger
    {
        protected FormattedLogger()
        {
            Level = LogLevel.Debug;
        }

        private readonly string[] m_levelString =
            new[]
            {
                "?????",
                "Debug",
                "Info ",
                "Warn ",
                "Error",
                "Fatal",
            };

        public void Write(string message, LogLevel logLevel)
        {
            if ((int)logLevel < (int)Level) return;

            var now = DateTimeOffset.UtcNow;

            string headerString = String.Format("{0} [{1}]: ", now.ToString("s"), m_levelString[(int)logLevel]);
            var rawLines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var lines = new StringBuilder();
            lines.AppendLine(headerString + rawLines[0]);
            var padding = new string(' ', headerString.Length);
            for (int i = 1; i < rawLines.Length; ++i)
                lines.AppendLine(padding + rawLines[i]);

            WriteFormatted(now, logLevel, lines.ToString());
        }

        /// <summary>
        /// Minimum log level.
        /// </summary>
        public LogLevel Level { get; set; }

        protected abstract void WriteFormatted(DateTimeOffset msgTime, LogLevel logLevel, string formattedMsg);
    }
}