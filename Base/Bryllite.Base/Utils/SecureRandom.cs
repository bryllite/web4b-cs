using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Bryllite
{
    public static class SecureRandom
    {
        // crypto random number generator
        private static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        /// <summary>
        /// get random byte array
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] GetBytes(int length)
        {
            if (length <= 0) return new byte[0];

            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return bytes;
        }

        /// <summary>
        /// get random byte array ( non-zero value each byte )
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] GetNonZeroBytes(int length)
        {
            if (length <= 0) return new byte[0];

            var bytes = new byte[length];
            rng.GetNonZeroBytes(bytes);
            return bytes;
        }

        public static byte GetByte()
        {
            var bytes = new byte[1];
            rng.GetBytes(bytes);
            return bytes[0];
        }

        public static byte GetNonZeroByte()
        {
            var bytes = new byte[1];
            rng.GetNonZeroBytes(bytes);
            return bytes[0];
        }

        private static int NextInt32()
        {
            unchecked
            {
                int first = Next(0, 1 << 4) << 28;
                int last = Next(0, 1 << 28);
                return first | last;
            }
        }


        /// <summary>
        /// get random value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Next<T>() where T : struct
        {
            if (typeof(T) == typeof(bool)) return (T)(object)(GetByte() % 2 == 0);
            else if (typeof(T) == typeof(byte)) return (T)(object)GetByte();
            else if (typeof(T) == typeof(sbyte)) return (T)(object)Hex.ToSByte(GetByte());
            else if (typeof(T) == typeof(short)) return (T)(object)BitConverter.ToInt16(GetBytes(2), 0);
            else if (typeof(T) == typeof(ushort)) return (T)(object)BitConverter.ToUInt16(GetBytes(2), 0);
            else if (typeof(T) == typeof(int)) return (T)(object)BitConverter.ToInt32(GetBytes(4), 0);
            else if (typeof(T) == typeof(uint)) return (T)(object)BitConverter.ToUInt32(GetBytes(4), 0);
            else if (typeof(T) == typeof(long)) return (T)(object)BitConverter.ToInt64(GetBytes(8), 0);
            else if (typeof(T) == typeof(ulong)) return (T)(object)BitConverter.ToUInt64(GetBytes(8), 0);
            else if (typeof(T) == typeof(float))
            {
                float f = BitConverter.ToSingle(GetBytes(4), 0);
                return float.IsInfinity(f) || float.IsNaN(f) ? Next<T>() : (T)(object)f;
            }
            else if (typeof(T) == typeof(double))
            {
                double d = BitConverter.ToDouble(GetBytes(8), 0);
                return double.IsInfinity(d) || double.IsNaN(d) ? Next<T>() : (T)(object)d;
            }
            else if (typeof(T) == typeof(decimal)) return (T)(object)new decimal(NextInt32(), NextInt32(), NextInt32(), Next<int>() % 2 == 0, (byte)Next((uint)0, 29));
            else throw new Exception("unsupported type");
        }

        public static short Next(short min, short max)
        {
            Guard.Assert(min < max);
            return (short)(min + (Math.Abs(Next<short>()) % (max - min)));
        }

        public static ushort Next(ushort min, ushort max)
        {
            Guard.Assert(min < max);
            return (ushort)(min + (Next<ushort>() % (max - min)));
        }

        public static int Next(int min, int max)
        {
            Guard.Assert(min < max);
            return (min + (Math.Abs(Next<int>()) % (max - min)));
        }

        public static uint Next(uint min, uint max)
        {
            Guard.Assert(min < max);
            return (min + (Next<uint>() % (max - min)));
        }

        public static long Next(long min, long max)
        {
            Guard.Assert(min < max);
            return (min + (Math.Abs(Next<long>()) % (max - min)));
        }

        public static ulong Next(ulong min, ulong max)
        {
            Guard.Assert(min < max);
            return (min + (Next<ulong>() % (max - min)));
        }
    }
}
