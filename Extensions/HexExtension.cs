using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Extensions
{
    public static class HexExtension
    {
        public static bool IsHexString(this string hex)
        {
            return Hex.IsHexString(hex);
        }

        public static string ToHexString(this byte[] bytes)
        {
            return Hex.ToString(bytes);
        }

        public static string ToHexString(this byte[] bytes, bool trim)
        {
            return Hex.ToString(bytes, trim);
        }

        public static string ToHexString<T>(this T number) where T : struct
        {
            return Hex.ToString(number);
        }

        public static string ToHexString<T>(this T number, bool trim) where T : struct
        {
            return Hex.ToString(number, trim);
        }

        public static byte[] ToByteArray(this string hex)
        {
            return Hex.ToByteArray(hex);
        }

        public static byte[] ToByteArray(this string hex, bool trim)
        {
            return Hex.ToByteArray(hex, trim);
        }

        public static byte[] ToByteArray<T>(this T number) where T : struct
        {
            return Hex.ToByteArray(number);
        }

        public static byte[] ToByteArray<T>(this T number, bool trim) where T : struct
        {
            return Hex.ToByteArray(number, trim);
        }

        public static T ToNumber<T>(this byte[] bytes) where T : struct
        {
            return Hex.ToNumber<T>(bytes);
        }

        public static T ToNumber<T>(this string hex) where T : struct
        {
            return Hex.ToNumber<T>(hex);
        }
    }
}
