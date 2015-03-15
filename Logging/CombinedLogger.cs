using Splat;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Creates a logger that combines multiple loggers.
    /// </summary>
    public class CombinedLogger : ILogger
    {
        private CombinedLogger(ILogger[] loggers)
        {
            m_loggers = loggers;
        }

        public static CombinedLogger Create(params ILogger[] loggers)
        {
            return new CombinedLogger(loggers);
        }

        private readonly ILogger[] m_loggers;
        private LogLevel m_level;

        #region Implementation of ILogger

        public void Write( string message, LogLevel logLevel )
        {
            foreach (var item in m_loggers)
                item.Write(message, logLevel);
        }

        public LogLevel Level
        {
            get { return m_level; }
            set
            {
                m_level = value;
                foreach ( var item in m_loggers )
                    item.Level = value;
            }
        }

        #endregion
    }
}