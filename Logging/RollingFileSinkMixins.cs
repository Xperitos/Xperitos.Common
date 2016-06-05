using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Xperitos.Common.Logging
{
    public static class RollingFileSinkMixins
    {
        public static LoggerConfiguration XpRollingFile(
            this LoggerSinkConfiguration sinkConfiguration,
            string pathFormat, 
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
            string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}",
            IFormatProvider formatProvider = null, 
            long? fileSizeLimitBytes = 100 * 1024 * 1024,
            int? retainedFileCountLimit = 60)
        {
            MessageTemplateTextFormatter templateTextFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            RollingFileSink sink = new RollingFileSink(pathFormat, templateTextFormatter, fileSizeLimitBytes, retainedFileCountLimit);

            return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }
    }
}