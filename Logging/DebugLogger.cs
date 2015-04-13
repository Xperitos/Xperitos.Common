using System;
using System.Diagnostics;
using Splat;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Log using the <see cref="Debug"/> facilities.
    /// </summary>
    public class DebugLogger : FormattedLogger
    {
        protected override void WriteFormatted(DateTimeOffset msgTime, LogLevel logLevel, string formattedMsg)
        {
            Debug.Write(formattedMsg);
        }
    }
}
