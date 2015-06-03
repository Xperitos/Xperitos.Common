using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    public static class FileUtils
    {
        /// <summary>
        /// Checks whether file is locked
        /// </summary>
        /// <param name="pathToFile">Path to file</param>
        /// <returns>Returns true if file is locked</returns>
        public static bool IsFileLocked(string pathToFile)
        {
            // If file doesn't exist then it's not locked...
            if (!File.Exists(pathToFile))
                return false;

            try
            {
                using (Stream stream = new FileStream(pathToFile, FileMode.Open))
                {
                    // File is accessible. Great!
                }
            }
            catch
            {
                // Got exception, then assume file is locked
                return true;
            }

            //file is not locked
            return false;
        }
    }
}
