using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting;
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
            var sink = new CountRollingFileSink(pathFormat, templateTextFormatter, fileSizeLimitBytes, retainedFileCountLimit);

            return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }

        public static LoggerConfiguration XpRollingFileByDate(
	        this LoggerSinkConfiguration sinkConfiguration,
	        string pathFormat,
	        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
	        string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}",
	        IFormatProvider formatProvider = null,
	        long? fileSizeLimitBytes = 100 * 1024 * 1024,
	        int retainedDays = 60,
	        long? maxRetainedTotalFileSizeBytes = 2 * 1024 * 1024 * 1024L)
        {
	        MessageTemplateTextFormatter templateTextFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
	        var sink = new DateRollingFileSink(pathFormat, templateTextFormatter, fileSizeLimitBytes, retainedDays, maxRetainedTotalFileSizeBytes);

	        return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }

        public static LoggerConfiguration XpRollingFileByDate(
	        this LoggerSinkConfiguration sinkConfiguration,
	        string pathFormat,
	        ITextFormatter formatter,
	        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
	        long? fileSizeLimitBytes = 100 * 1024 * 1024,
	        int retainedDays = 60,
	        long? maxRetainedTotalFileSizeBytes = 2 * 1024 * 1024 * 1024L)
        {
	        var sink = new DateRollingFileSink(pathFormat, formatter, fileSizeLimitBytes, retainedDays, maxRetainedTotalFileSizeBytes);

	        return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }
    }
}