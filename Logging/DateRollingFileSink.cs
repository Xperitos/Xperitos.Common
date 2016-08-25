using System;
using System.IO;
using System.Linq;
using System.Text;
using Serilog.Formatting;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Implements a rolling file sink that all files by total size and date (whichever comes first).
    /// </summary>
    sealed class DateRollingFileSink : RollingFileSinkBase
    {
        public DateRollingFileSink(
            string pathFormat, 
            ITextFormatter textFormatter,
            long? fileSizeLimitBytes,
            int retainedDays,
            long? maxRetainedTotalFileSizeBytes = null,
            Encoding encoding = null) 
            : base(pathFormat, textFormatter, encoding)
        {
            if (fileSizeLimitBytes.HasValue && fileSizeLimitBytes < 0) throw new ArgumentException("Negative value provided; file size limit must be non-negative", nameof(fileSizeLimitBytes));
            if (maxRetainedTotalFileSizeBytes.HasValue && maxRetainedTotalFileSizeBytes < 0) throw new ArgumentException("Zero or negative value provided; max retained total file size must be at least 1", nameof(maxRetainedTotalFileSizeBytes));

            m_fileSizeLimitBytes = fileSizeLimitBytes;
            m_retainedDays = retainedDays;
            m_maxRetainedTotalFileSizeBytes = maxRetainedTotalFileSizeBytes;
        }

        private readonly long? m_fileSizeLimitBytes;
        private readonly int m_retainedDays;
        private readonly long? m_maxRetainedTotalFileSizeBytes;


        protected override bool ShouldRollFile(DateTime now, SizedFileSink currentFile)
        {
            // Close previous file if size reached.
            return m_fileSizeLimitBytes.HasValue && currentFile.EstimatedLength >= m_fileSizeLimitBytes.Value;
        }
        protected override void ApplyRetentionPolicy(DateTime now, string currentFilename)
        {
            var oldestDateToKeep = now.Date.AddDays(-m_retainedDays);

            var allFiles = EnumerateFiles()
                .Where(v => String.Compare(v.FullName, currentFilename, StringComparison.CurrentCultureIgnoreCase) != 0)
                .ToList();

            int keepByDateCount = allFiles.Count(v => v.Date >= oldestDateToKeep);

            long fileSizeSum = 0;
            int keepByFileSizeCount = 0;
            if (!m_maxRetainedTotalFileSizeBytes.HasValue)
                keepByFileSizeCount = allFiles.Count;
            else
            {
                for (keepByFileSizeCount = 0; keepByFileSizeCount < allFiles.Count; ++keepByFileSizeCount)
                {
                    var item = allFiles[keepByFileSizeCount];
                    long length;
                    try
                    {
                        length = new FileInfo(item.FullName).Length;
                    }
                    catch (Exception)
                    {
                        // Ignore exception.
                        length = 0;
                    }

                    fileSizeSum += length;

                    if (fileSizeSum > m_maxRetainedTotalFileSizeBytes.Value)
                        break;
                }
            }

            var filesToRemove = allFiles.Skip(Math.Min(keepByDateCount, keepByFileSizeCount));
            foreach (var file in filesToRemove)
            {
                try
                {
                    File.Delete(file.FullName);
                }
                catch (Exception)
                {
                    // Where should this be logged!?
                }
            }
        }
    }
}