using System.Diagnostics;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Log using the <see cref="Debug"/> facilities.
    /// </summary>
    public class DebugLogger : FormattedLogger
    {
        protected override void WriteFormatted(string formattedMsg)
        {
            Debug.Write(formattedMsg);
        }
    }
}
