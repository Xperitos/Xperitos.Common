using System;
using System.Collections.Generic;
using System.Text;

namespace Xperitos.Common.Utils
{
    public static class OSDetector
    {
        public enum OSFamily
        {
            Unknown,
            Windows,
            Linux
        }

        public static OSFamily CurrentOS
        {
            get
            {
                OperatingSystem os = Environment.OSVersion;
                PlatformID pid = os.Platform;
                switch (pid)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                        return OSFamily.Windows;
                    case PlatformID.Unix:
                        return OSFamily.Linux;
                    default:
                        return OSFamily.Unknown;
                }
            }
        }
    }
}
