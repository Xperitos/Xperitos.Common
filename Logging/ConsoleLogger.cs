using System;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Log using the <see cref="Console"/> facilities.
    /// </summary>
    public class ConsoleLogger : FormattedLogger
    {
        protected override void WriteFormatted(string formattedMsg)
        {
            Console.Write(formattedMsg);
        }
    }
}