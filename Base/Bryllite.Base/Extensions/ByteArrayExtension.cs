using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Bryllite.Extensions
{
    public static class ByteArrayExtension
    {
        // to hash code for GetHashCode
        public static int ToHashCode(this IEnumerable<byte> bytes)
        {
            return Hex.IsNullOrEmpty(bytes) ? 0 : new BigInteger(bytes.ToArray()).GetHashCode();
        }

        // trim leading zero bytes
        // return byte[1] {0} if all bytes are zero bytes
        public static byte[] Trim(this IEnumerable<byte> bytes)
        {
            if (Hex.IsNullOrEmpty(bytes)) return new byte[0];

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
