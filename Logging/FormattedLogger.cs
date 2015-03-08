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
        public void Write(string message, LogLevel logLevel)
        {
            if ((int)logLevel < (int)Level) return;

            string msg = String.Format("{0}: {1}{2}", DateTimeOffset.UtcNow.ToString("s"), message, Environment.NewLine);
            WriteFormatted(msg);
        }

        public LogLevel Level { get; set; }

        protected abstract void WriteFormatted(string formattedMsg);
    }
}