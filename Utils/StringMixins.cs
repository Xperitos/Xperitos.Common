using System;
using System.Collections.Generic;
using System.Linq;

namespace Xperitos.Common.Utils
{
    public static class StringMixins
    {
        public static string ToHexString(this IEnumerable<byte> msg)
        {
            return String.Join(" ", msg.Select(b => b.ToString("X2")));
        }
    }
}
