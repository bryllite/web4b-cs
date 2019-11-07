using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bryllite.Extensions
{
    public static class ByteArrayExtension
    {
        // is null or empty?
        public static bool IsNullOrEmpty(this IEnumerable<byte> bytes)
        {
            return ReferenceEquals(bytes, null) || bytes.Count() == 0;
        }

        // trim leading zero bytes
        // return byte[1] {0} if all bytes are zero bytes
        public static byte[] Trim(this IEnumerable<byte> bytes)
        {
            if (bytes.IsNullOrEmpty()) return new byte[0];

            int skip = 0;
            foreach (var b in bytes)
            {
                if (b != 0) break;
                skip++;
            }

            byte[] trimmed = bytes.Skip(skip).ToArray();
            return trimmed.Length > 0 ? trimmed : new byte[1];
        }
    }
}
