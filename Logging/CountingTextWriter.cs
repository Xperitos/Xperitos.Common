using System.IO;
using System.Text;
using System.Threading;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Text writer that provides a count for written bytes.
    /// </summary>
    sealed class CountingTextWriter : TextWriter
    {
        public CountingTextWriter(TextWriter outputWriter)
        {
            m_outputWriter = outputWriter;
        }

        /// <summary>
        /// Number of chars wrote.
        /// </summary>
        public long WroteChars => m_wroteChars;
        private long m_wroteChars;

        private readonly TextWriter m_outputWriter;

        public override Encoding Encoding => m_outputWriter.Encoding;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                m_outputWriter.Dispose();

            base.Dispose(disposing);
        }

        public override void Write(char value)
        {
            m_outputWriter.Write(value);
            Interlocked.Increment(ref m_wroteChars);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            m_outputWriter.Write(buffer, index, count);
            Interlocked.Add(ref m_wroteChars, count);
        }

        public override void Flush()
        {
            m_outputWriter.Flush();
        }

    }
}