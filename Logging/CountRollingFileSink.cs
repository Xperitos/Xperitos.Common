using System;
using System.IO;
using System.Linq;
using System.Text;
using Serilog.Formatting;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Implements a rolling file sink that maintains a list of files of maximum size.
    /// </summary>
    sealed class CountRollingFileSink : RollingFileSinkBase
    {
        public CountRollingFileSink(
            string pathFormat, 
            ITextFormatter textFormatter, 
            long? fileSizeLimitBytes, 
            int? retainedFileCountLimit, 
            Encoding encoding = null) 
            : base(pathFormat, textFormatter, encoding)
        {
            if (fileSizeLimitBytes.HasValue && fileSizeLimitBytes < 0) throw new ArgumentException("Negative value provided; file size limit must be non-negative", nameof(fileSizeLimitBytes));
            if (retainedFileCountLimit.HasValue && retainedFileCountLimit < 1) throw new ArgumentException("Zero or negative value provided; retained file count limit must be at least 1", nameof(retainedFileCountLimit));

            m_fileSizeLimitBytes = fileSizeLimitBytes;
            m_retainedFileCountLimit = retainedFileCountLimit;
        }

        private readonly long? m_fileSizeLimitBytes;
        private readonly int? m_retainedFileCountLimit;

        protected override bool ShouldRollFile(DateTime now, SizedFileSink currentFile)
        {
            // Close previous file if size reached.
            return m_fileSizeLimitBytes.HasValue && currentFile.EstimatedLength >= m_fileSizeLimitBytes.Value;
        }
        protected override void ApplyRetentionPolicy(DateTime now, string currentFilename)
        {
            if (m_retainedFileCountLimit == null)
                return;

            var filesToRemove = EnumerateFiles()
                .Skip(m_retainedFileCountLimit.Value - 1)
                .Where(v => String.Compare(v.FullName, currentFilename, StringComparison.CurrentCultureIgnoreCase) != 0)
                .ToList();

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