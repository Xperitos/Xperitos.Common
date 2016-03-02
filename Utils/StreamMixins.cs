using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    public static class StreamMixins
    {
        public static async Task<string> ReadAllAsync(this Stream stream, Encoding encoding)
        {
            using (var mem = new System.IO.MemoryStream())
            {
                await stream.CopyToAsync(mem).ConfigureAwait(false);
                var bytes = mem.ToArray();
                return encoding.GetString(bytes);
            }
        }
        public static Task<string> ReadAllAsync(this Stream stream)
        {
            return ReadAllAsync(stream, Encoding.UTF8);
        }
    }
}
