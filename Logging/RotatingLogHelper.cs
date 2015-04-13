using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Xperitos.Common.Logging
{
    /// <summary>
    /// Helper class to write to a rotating log file.
    /// </summary>
    /// <remarks>NOT THREAD SAFE!!!</remarks>
    public class RotatingLogHelper
    {
        public RotatingLogHelper(string logPath, string filenameBase, string logFileExt, int maxLogFiles, Func<RotatingLogHelper, byte[], bool> rotationPolicyFunc)
        {
            m_logFileExt = logFileExt;
            m_maxLogFiles = maxLogFiles;
            m_rotationPolicyFunc = rotationPolicyFunc;
            m_logPath = Directory.CreateDirectory(logPath);
            m_logFilePattern = Path.GetFileName(filenameBase);

            m_logFilename = Path.Combine(m_logPath.FullName, m_logFilePattern + logFileExt);

            // Initialize the filesize for bookkeeping.
            if (File.Exists(m_logFilename))
            {
                FileInfo fi = new FileInfo(m_logFilename);
                m_currentLogFileSize = fi.Length;
                m_currentLogFileCreationTime = fi.CreationTimeUtc;
            }
        }

        private readonly int m_maxLogFiles;
        private readonly Func<RotatingLogHelper, byte[], bool> m_rotationPolicyFunc;

        private readonly DirectoryInfo m_logPath;
        private readonly string m_logFileExt;
        private readonly string m_logFilePattern;
        private readonly string m_logFilename;

        private long m_currentLogFileSize;
        private DateTimeOffset m_currentLogFileCreationTime;

        /// <summary>
        /// Write the specified message
        /// </summary>
        /// <param name="msg">String to write</param>
        /// <param name="encoding">Encoding to use when writing the string. null means UTF8</param>
        /// <returns>Return true if the log file was rotated after msg was written.</returns>
        public bool Write(string msg, Encoding encoding = null)
        {
            var msgBytes = (encoding ?? Encoding.UTF8).GetBytes(msg);
            return Write(msgBytes);
        }

        /// <summary>
        /// Write the specified message
        /// </summary>
        /// <param name="msgBytes">bytes to write</param>
        /// <returns>Return true if the log file was rotated before msg was written.</returns>
        public bool Write(byte[] msgBytes)
        {
            bool result = false;
            if (m_rotationPolicyFunc(this, msgBytes))
                result = RotateFile();

            using (var writer = new FileStream(m_logFilename, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                writer.Write(msgBytes, 0, msgBytes.Length);
                m_currentLogFileSize += msgBytes.Length;
            }

            return result;
        }

        public long CurrentLogFileSize { get { return m_currentLogFileSize; } }
        public DateTimeOffset CurrentLogFileCreationTime { get { return m_currentLogFileCreationTime; } }

        /// <summary>
        /// Implicitly rotate the file. Return true if file was rotated.
        /// </summary>
        /// <returns></returns>
        public bool RotateFile()
        {
            var allLogs =
                m_logPath
                    .EnumerateFiles(m_logFilePattern + ".??" + m_logFileExt)
                    .OrderBy(f => f.Name)
                    .ToArray();
            try
            {
                // Remove the previous logs. Keep only the latest N
                foreach (var f in allLogs.Skip(m_maxLogFiles - 2))
                    f.Delete();

                string newName;
                foreach (var f in allLogs.Reverse())
                {
                    var name = f.Name;
                    var idxNumberString = name.Substring(name.Length - m_logFileExt.Length - 2, 2);
                    long index = int.Parse(idxNumberString);

                    newName = Path.Combine(
                        m_logPath.FullName,
                        m_logFilePattern + String.Format(".{0:D2}", index + 1) + m_logFileExt);
                    f.MoveTo(newName);
                }

                newName = Path.Combine(
                    m_logPath.FullName,
                    m_logFilePattern + ".01" + m_logFileExt);
                File.Move(m_logFilename, newName);

                m_currentLogFileSize = 0;
                m_currentLogFileCreationTime = DateTimeOffset.UtcNow;
                return true;
            }
            catch (Exception)
            {
                // Ignore the error and keep writing to the same file.
                return false;
            }
        }
    }
}
