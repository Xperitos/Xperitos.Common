using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Xperitos.Common.Logging
{
    public class RotatingLogFileLogger : RotatingLogFileLoggerBase
    {
        /// <summary>
        /// Construct a new simple file logger - rotates when certain file size is reached.
        /// </summary>
        /// <param name="filenameBase">Directory + base name for the logs</param>
        /// <param name="maxFileSize">Max file size before rotation occurs</param>
        /// <param name="maxLogFiles">Maximum back log files to keep.</param>
        public RotatingLogFileLogger(string filenameBase, long maxFileSize = 1 * 1024 * 1024, int maxLogFiles = 10)
            : base(filenameBase, maxLogFiles)
        {
            m_maxFileSize = maxFileSize;
        }

        private readonly long m_maxFileSize;

        protected override bool ShouldRotate(RotatingLogHelper helper, byte[] bytes)
        {
            return helper.CurrentLogFileSize + bytes.Length > m_maxFileSize;
        }
    }
}
