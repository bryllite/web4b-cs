using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Bryllite
{
    // byte[] class for convert hex string, byte[], numbers
    public class Hex : ICloneable, IComparable, IComparable<Hex>, IEnumerable, IEnumerable<byte>
    {
        public static readonly Hex Null = null;
        public static readonly Hex Empty = new Hex(0);
        public static readonly Hex Zero = new Hex(1);

        // is little endian?
        public static bool IsLittleEndian => BitConverter.IsLittleEndian;

        // hex prefix
        public static readonly string Prefix = "0x";

        // hex characters
        public static readonly string Chars = "0123456789abcdefABCDEF";

        // byte[] value
        protected byte[] value;
        public byte[] Value => value;

        // byte length 
        public int Length => value.Length;

        // enumerator
        public IEnumerator<byte> GetEnumerator()
        {
            return new List<byte>(value).GetEnumerator();
        }

        // enumerator
        IEnumerator IEnumerable.GetEnumerator()
        {
            return value.GetEnumerator();
        }

        // indexer
        public byte this[int idx] => value[idx];


        protected Hex(int bytesLength)
        {
            value = new byte[bytesLength];
        }

        protected Hex(byte[] bytes)
        {
            value = bytes ?? new byte[0];
        }

        protected Hex(string hex) : this(ToByteArray(hex))
        {
        }

        // hex string
        public override string ToString()
        {
            return ToString(value);
        }

        public string ToString(bool trim)
        {
            return ToString(value, trim);
        }

        // to byte array
        public byte[] ToByteArray()
        {
            return value;
        }

        // to byte array
        public byte[] ToByteArray(bool trim)
        {
            return Trim(value);
        }

        // to number type
        public T ToNumber<T>() where T : struct
        {
            return ToNumber<T>(value);
        }

        public Hex Clone()
        {
            return new Hex(value.ToArray());
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public override int GetHashCode()
        {
            return new BigInteger(value).GetHashCode();
        }

        /// <summary>
        /// 0x0012 와 0x12는 Equals()에서는 다르다 ( false )
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (ReferenceEquals(obj, null)) return false;

            return (obj is IEnumerable<byte> bytes) ? value.SequenceEqual(bytes) : false;
        }

        /// <summary>
        /// 0x0012와 0x12는 CompareTo()에서는 같다 ( == 0 )
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>
        /// 1 : if this is greater than bytes
        /// 0 : if this equals bytes
        /// -1 : if this is less than bytes
        /// </returns>
        public int CompareTo(IEnumerable<byte> bytes)
        {
            if (IsNullOrEmpty(bytes)) return -1;

            // remove leading zero bytes
            var left = Trim(value);
            var right = Trim(bytes);

            int result = left.Length.CompareTo(right.Length);
            if (result == 0)
            {
                for (int i = 0; i < left.Length && result == 0; i++)
                    result = left[i].CompareTo(right[i]);
            }

            return result == 0 ? result : result > 0 ? 1 : -1;
        }

        /// <summary>
        /// Hex, byte[], string(HexString) 과 같은지 비교한다.
        /// 0x0012 와 0x12는 CompareTo()에서는 == 0
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int IComparable.CompareTo(object o)
        {
            if (o is IEnumerable<byte> bytes)
                return CompareTo(bytes);
            else if (o is string str)
                return CompareTo(ToByteArray(str, true));

            throw new Exception("unsupported type!");
        }

        int IComparable<Hex>.CompareTo(Hex other)
        {
            return CompareTo(other.value);
        }

        public static bool operator ==(Hex left, Hex right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.CompareTo(right) == 0;
        }

        public static bool operator !=(Hex left, Hex right)
        {
            return !(left == right);
        }


        // 크다, 작다를 비교할 때는 리딩 제로를 제외한 나머지로 비교한다.
        // 즉, "0x00001234"와 "0x001234" 는 같다
        // "0x00001234"는 "0x1235" 보다 작다
        // [CAUTION] >, >=, <, <=, CompareTo 등은 byte[] 상태에서의 비교로
        // Number Type 상태의 크기 비교와 동일함을 보장하지 않는다!!!
        public static bool operator >(Hex left, Hex right)
        {
            if (ReferenceEquals(left, right)) return false;
            if (ReferenceEquals(left, null)) return false;
            if (ReferenceEquals(right, null)) return true;
            return left.CompareTo(right) > 0;
        }

        public static bool operator <(Hex left, Hex right)
        {
            if (ReferenceEquals(left, right)) return false;
            if (ReferenceEquals(left, null)) return true;
            if (ReferenceEquals(right, null)) return false;
            return left.CompareTo(right) < 0;
        }

        public static bool operator >=(Hex left, Hex right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            if (ReferenceEquals(right, null)) return true;
            return left.CompareTo(right) >= 0;
        }
        public static bool operator <=(Hex left, Hex right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return true;
            if (ReferenceEquals(right, null)) return false;
            return left.CompareTo(right) <= 0;
        }

        // sbyte to byte
        public static byte ToByte(sbyte sb)
        {
            byte[] arr = new byte[1];
            Buffer.BlockCopy(new sbyte[] { sb }, 0, arr, 0, 1);
            return arr[0];
        }

        // byte to sbyte
        public static sbyte ToSByte(byte b)
        {
            sbyte[] arr = new sbyte[1];
            Buffer.BlockCopy(new byte[] { b }, 0, arr, 0, 1);
            return arr[0];
        }

        // has hex prefix?
        public static bool HasPrefix(string hex)
        {
            return !string.IsNullOrEmpty(hex) ? hex.ToLower().StartsWith(Prefix) : false;
        }

        // remove hex prefix
        public static string StripPrefix(string hex)
        {
            return !HasPrefix(hex) ? hex : hex.Substring(Prefix.Length, hex.Length - Prefix.Length);
        }

        // is hex string?
        public static bool IsHexString(string str)
        {
            if (!HasPrefix(str) || str.Length % 2 != 0) return false;
            return StripPrefix(str).All(Chars.Contains);
        }

        // byte array를 hex string으로 변환한다
        public static string ToString(byte[] bytes)
        {
            return IsNullOrEmpty(bytes) ? string.Empty : Prefix + string.Concat(bytes.Select(b => b.ToString("x2")).ToArray());
        }

        public static string ToString(byte[] bytes, bool trim)
        {
            return ToString(trim ? Trim(bytes) : bytes);
        }

        // number를 hex string으로 변환한다
        public static string ToString<T>(T number) where T : struct
        {
            return ToString(ToByteArray(number));
        }

        public static string ToString<T>(T number, bool trim) where T : struct
        {
            return ToString(trim ? Trim(ToByteArray(number)) : ToByteArray(number));
        }

        // hex string을 byte array로 변환한다
        public static byte[] ToByteArray(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return new byte[0];
            if (!IsHexString(hex)) throw new ArgumentException("not hex string");

            List<byte> bytes = new List<byte>();

            string str = StripPrefix(hex);
            for (int i = 0; i < str.Length; i += 2)
                bytes.Add(Convert.ToByte(str.Substring(i, 2), 16));

            return bytes.ToArray();
        }

        public static byte[] ToByteArray(string hex, bool trim)
        {
            return trim ? Trim(ToByteArray(hex)) : ToByteArray(hex);
        }

        /// <summary>
        /// number type을 byte array로 변환한다. 이때 byte array는 BigEndian이다
        /// </summary>
        /// <typeparam name="T">one of type [bool, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal]</typeparam>
        /// <param name="number"></param>
        /// <returns>byte[] for the number, BigEndian</returns>
        public static byte[] ToByteArray<T>(T number) where T : struct
        {
            object o = number;
            byte[] numberBytes = null;

            try
            {
                if (o is bool b) return BitConverter.GetBytes(b);
                else if (o is byte by) return new byte[] { by };
                else if (o is sbyte sb) return new byte[] { ToByte(sb) };
                else if (o is short s) numberBytes = BitConverter.GetBytes(s);
                else if (o is ushort us) numberBytes = BitConverter.GetBytes(us);
                else if (o is int i) numberBytes = BitConverter.GetBytes(i);
                else if (o is uint ui) numberBytes = BitConverter.GetBytes(ui);
                else if (o is long l) numberBytes = BitConverter.GetBytes(l);
                else if (o is ulong ul) numberBytes = BitConverter.GetBytes(ul);
                else if (o is float f) numberBytes = BitConverter.GetBytes(f);
                else if (o is double d) numberBytes = BitConverter.GetBytes(d);
                else if (o is decimal de)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (BinaryWriter wr = new BinaryWriter(ms))
                        {
                            wr.Write(de);
                            numberBytes = ms.ToArray();
                        }
                    }
                }
                else if (o is BigInteger bi) numberBytes = bi.ToByteArray();
                else throw new TypeAccessException("unsupported type");
            }
            finally
            {
                // convert to BigEndian
                if (!IsNullOrEmpty(numberBytes) && IsLittleEndian)
                    numberBytes = numberBytes.Reverse().ToArray();
            }

            return numberBytes;
        }

        public static byte[] ToByteArray<T>(T number, bool trim) where T : struct
        {
            return trim ? Trim(ToByteArray(number)) : ToByteArray(number);
        }

        /// <summary>
        /// byte array를 number type으로 변환한다. 
        /// </summary>
        /// <typeparam name="T">one of type [bool, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal]</typeparam>
        /// <param name="bytes">BigEndian으로 간주한다</param>
        /// <returns></returns>
        public static T ToNumber<T>(byte[] bytes) where T : struct
        {
            if (IsNullOrEmpty(bytes)) return default(T);

            // number bytes
            byte[] numberBytes = IsLittleEndian ? bytes.Reverse().ToArray() : bytes;

            if (typeof(T) == typeof(bool)) return (T)(object)BitConverter.ToBoolean(numberBytes, 0);
            else if (typeof(T) == typeof(byte)) return (T)(object)numberBytes[0];
            else if (typeof(T) == typeof(sbyte)) return (T)(object)ToSByte(numberBytes[0]);
            else if (typeof(T) == typeof(short))
            {
                int len = sizeof(short);
                byte[] buffer = new byte[len];
                Array.Copy(numberBytes, buffer, Math.Min(numberBytes.Length, len));
                return (T)(object)BitConverter.ToInt16(buffer, 0);
            }
            else if (typeof(T) == typeof(ushort))
            {
                int len = sizeof(ushort);
                byte[] buffer = new byte[len];
                Array.Copy(numberBytes, buffer, Math.Min(numberBytes.Length, len));
                return (T)(object)BitConverter.ToUInt16(buffer, 0);
            }
            else if (typeof(T) == typeof(int))
            {
                int len = sizeof(int);
                byte[] buffer = new byte[len];
                Array.Copy(numberBytes, buffer, Math.Min(numberBytes.Length, len));
                return (T)(object)BitConverter.ToInt32(buffer, 0);
            }
            else if (typeof(T) == typeof(uint))
            {
                int len = sizeof(uint);
                byte[] buffer = new byte[len];
                Array.Copy(numberBytes, buffer, Math.Min(numberBytes.Length, len));
                return (T)(object)BitConverter.ToUInt32(buffer, 0);
            }
            else if (typeof(T) == typeof(long))
            {
                int len = sizeof(long);
                byte[] buffer = new byte[len];
                Array.Copy(numberBytes, buffer, Math.Min(numberBytes.Length, len));
                return (T)(object)BitConverter.ToInt64(buffer, 0);
            }
            else if (typeof(T) == typeof(ulong))
            {
                int len = sizeof(ulong);
                byte[] buffer = new byte[len];
                Array.Copy(numberBytes, buffer, Math.Min(numberBytes.Length, len));
                return (T)(object)BitConverter.ToUInt64(buffer, 0);
            }
            else if (typeof(T) == typeof(float))
            {
                int len = sizeof(float);
                byte[] buffer = new byte[len];
                Array.Copy(numberBytes, buffer, Math.Min(numberBytes.Length, len));
                return (T)(object)BitConverter.ToSingle(buffer, 0);
            }
            else if (typeof(T) == typeof(double))
            {
                int len = sizeof(double);
                byte[] buffer = new byte[len];
                Array.Copy(numberBytes, buffer, Math.Min(numberBytes.Length, len));
                return (T)(object)BitConverter.ToDouble(buffer, 0);
            }
            else if (typeof(T) == typeof(decimal))
            {
                int len = sizeof(decimal);
                byte[] buffer = new byte[len];
                Array.Copy(numberBytes, buffer, Math.Min(numberBytes.Length, len));

                using (MemoryStream ms = new MemoryStream(buffer))
                using (BinaryReader rd = new BinaryReader(ms))
                    return (T)(object)rd.ReadDecimal();
            }
            else if (typeof(T) == typeof(BigInteger))
            {
                return (T)(object)new BigInteger(numberBytes);
            }

            throw new TypeAccessException("unsupported type");
        }

        // hex string to number
        public static T ToNumber<T>(string hex) where T : struct
        {
            return ToNumber<T>(ToByteArray(hex));
        }


        public static Hex Parse(byte[] bytes)
        {
            return new Hex(bytes);
        }

        public static Hex Parse(string hex)
        {
            return new Hex(hex);
        }

        public static Hex Parse<T>(T value) where T : struct
        {
            return Parse(ToByteArray(value, true));
        }

        public static bool TryParse(byte[] bytes, out Hex hex)
        {
            try
            {
                hex = Parse(bytes);
                return true;
            }
            catch
            {
                hex = Null;
                return false;
            }
        }

        public static bool TryParse(string str, out Hex hex)
        {
            try
            {
                hex = Parse(str);
                return true;
            }
            catch
            {
                hex = Null;
                return false;
            }
        }

        public static bool TryParse<T>(T value, out Hex hex) where T : struct
        {
            try
            {
                hex = Parse(value);
                return true;
            }
            catch
            {
                hex = Null;
                return false;
            }
        }


        public static implicit operator Hex(byte[] bytes)
        {
            return TryParse(bytes, out var hex) ? hex : Null;
        }

        public static implicit operator Hex(string str)
        {
            return TryParse(str, out var hex) ? hex : Null;
        }

        public static implicit operator byte[] (Hex hex)
        {
            return hex?.value;
        }

        public static implicit operator string(Hex hex)
        {
            return hex?.ToString();
        }

        public static implicit operator Hex(bool value)
        {
            return TryParse(value, out var hex) ? hex : Null;
        }

        public static implicit operator Hex(byte value)
        {
            return TryParse(value, out var hex) ? hex : Null;
        }

        public static implicit operator Hex(sbyte value)
        {
            return TryParse(value, out var hex) ? hex : Null;
        }

        public static implicit operator Hex(short value)
        {
            return TryParse(value, out var hex) ? hex : Null;
        }

        public static implicit operator Hex(ushort value)
        {
            return TryParse(value, out var hex) ? hex : Null;
        }

        public static implicit operator Hex(int value)
        {
            return TryParse(value, out var hex) ? hex : Null;
        }

        public static implicit operator Hex(uint value)
        {
            return TryParse(value, out var hex) ? hex : Null;
        }

        public static implicit operator Hex(long value)
        {
            return TryParse(value, out var hex) ? hex : Null;
        }

        public static implicit operator Hex(ulong value)
        {
            return TryParse(value, out var hex) ? hex : Null;
        }

        public static implicit operator Hex(float value)
        {
            return TryParse(value, out var hex) ? hex : Null;
        }

        public static implicit operator Hex(double value)
        {
            return TryParse(value, out var hex) ? hex : Null;
        }

        public static implicit operator Hex(decimal value)
        {
            return TryParse(value, out var hex) ? hex : Null;
        }

        public static implicit operator bool(Hex hex)
        {
            return ToNumber<bool>(hex.value);
        }

        public static implicit operator byte(Hex hex)
        {
            return ToNumber<byte>(hex.value);
        }

        public static implicit operator sbyte(Hex hex)
        {
            return ToNumber<sbyte>(hex.value);
        }

        public static implicit operator short(Hex hex)
        {
            return ToNumber<short>(hex.value);
        }

        public static implicit operator ushort(Hex hex)
        {
            return ToNumber<ushort>(hex.value);
        }

        public static implicit operator int(Hex hex)
        {
            return ToNumber<int>(hex.value);
        }

        public static implicit operator uint(Hex hex)
        {
            return ToNumber<uint>(hex.value);
        }

        public static implicit operator long(Hex hex)
        {
            return ToNumber<long>(hex.value);
        }

        public static implicit operator ulong(Hex hex)
        {
            return ToNumber<ulong>(hex.value);
        }

        public static implicit operator float(Hex hex)
        {
            return ToNumber<float>(hex.value);
        }

        public static implicit operator double(Hex hex)
        {
            return ToNumber<double>(hex.value);
        }

        public static implicit operator decimal(Hex hex)
        {
            return ToNumber<decimal>(hex.value);
        }

        public static bool IsNullOrEmpty<T>(IEnumerable<T> array)
        {
            return ReferenceEquals(array, null) || array.Count() == 0;
        }

        public static byte[] Trim(IEnumerable<byte> bytes)
        {
            if (IsNullOrEmpty(bytes)) return Empty;

            int skip = 0;
            foreach (var b in bytes)
            {
                if (b != 0) break;
                skip++;
            }

            byte[] trimmed = bytes.Skip(skip).ToArray();
            return trimmed.Length > 0 ? trimmed : (byte[])Zero;
        }

    }
}
