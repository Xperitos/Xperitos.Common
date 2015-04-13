using System;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Log using the <see cref="Console"/> facilities.
    /// </summary>
    public class ConsoleLogger : FormattedLogger
    {
        protected override void WriteFormatted(DateTimeOffset msgTime, Splat.LogLevel logLevel, string formattedMsg)
        {
            Console.Write(formattedMsg);
        }
    }
}