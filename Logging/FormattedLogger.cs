using System;
using System.Diagnostics;
using Splat;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Base class for logger with fomatted message.
    /// </summary>
    public abstract class FormattedLogger : ILogger
    {
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

            string msg = String.Format("{0} [{1}]: {2}{3}", DateTimeOffset.UtcNow.ToString("s"), m_levelString[(int)logLevel], message, Environment.NewLine);
            WriteFormatted(msg);
        }

        public LogLevel Level { get; set; }

        protected abstract void WriteFormatted(string formattedMsg);
    }
}