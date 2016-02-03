using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    public class ConsoleUtils
    {
        static class Native
        {
            [DllImport("kernel32.dll")]
            public static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleModes dwMode);

            [DllImport("kernel32.dll")]
            public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleModes lpMode);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetStdHandle(int handle);

            public const int STD_INPUT_HANDLE = -10;

            [Flags]
            public enum ConsoleModes : uint
            {
                ENABLE_PROCESSED_INPUT = 0x0001,
                ENABLE_LINE_INPUT = 0x0002,
                ENABLE_ECHO_INPUT = 0x0004,
                ENABLE_WINDOW_INPUT = 0x0008,
                ENABLE_MOUSE_INPUT = 0x0010,
                ENABLE_INSERT_MODE = 0x0020,
                ENABLE_QUICK_EDIT_MODE = 0x0040,
                ENABLE_EXTENDED_FLAGS = 0x0080,
                ENABLE_AUTO_POSITION = 0x0100,
                ENABLE_PROCESSED_OUTPUT = 0x0001,
                ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
            }
        }

        /// <summary>
        /// Enable or disable the quick edit mode (mouse selection).
        /// </summary>
        public static bool IsQuickEditMode
        {
            get
            {
                Native.ConsoleModes mode;
                IntPtr handle = Native.GetStdHandle(Native.STD_INPUT_HANDLE);
                Native.GetConsoleMode(handle, out mode);
                return (mode & Native.ConsoleModes.ENABLE_QUICK_EDIT_MODE) != 0;
            }
            set
            {
                Native.ConsoleModes mode;
                IntPtr handle = Native.GetStdHandle(Native.STD_INPUT_HANDLE);
                Native.GetConsoleMode(handle, out mode);

                if (value)
                    mode |= Native.ConsoleModes.ENABLE_QUICK_EDIT_MODE | Native.ConsoleModes.ENABLE_EXTENDED_FLAGS;
                else
                    mode &= ~Native.ConsoleModes.ENABLE_QUICK_EDIT_MODE;
                Native.SetConsoleMode(handle, mode);
            }
        }
    }
}
