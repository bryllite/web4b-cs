using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Extensions
{
    public static class NibbleExtension
    {
        public static Nibble ToNibble(this byte b)
        {
            return new Nibble(b);
        }

        public static Nibble[] ToNibbleArray(this byte b)
        {
            return new Nibble[] { (byte)(b >> 4), (byte)(b % 16) };
        }

        public static Nibble[] ToNibbleArray(this byte[] bytes)
        {
            if (bytes.IsNullOrEmpty()) return new Nibble[0];

            List<Nibble> nibbles = new List<Nibble>();
            foreach (var b in bytes)
                nibbles.AddRange(b.ToNibbleArray());
            return nibbles.ToArray();
        }

        public static Nibble[] ToNibbleArray(this string hex)
        {
            return ToNibbleArray(hex.ToByteArray());
        }

        public static byte[] ToByteArray(this Nibble[] nibbles)
        {
            if (nibbles.IsNullOrEmpty()) return new byte[0];

            Guard.Assert(nibbles.Length % 2 == 0);

            List<byte> bytes = new List<byte>();
            for (int i = 0; i < nibbles.Length; i += 2)
                bytes.Add((byte)((nibbles[i] << 4) + nibbles[i + 1]));

            return bytes.ToArray();
        }

        public static string ToHexString(this Nibble[] nibbles)
        {
            if (nibbles.IsNullOrEmpty()) return string.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (var nibble in nibbles)
                sb.Append(nibble.ToString());

            return sb.ToString();
        }

    }
}
