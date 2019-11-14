using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Bryllite.Base.Tests
{
    public class HexTests
    {
        [Fact]
        public void HexTypeWithNullOrEmptyTest()
        {
            string strNull = null;
            string strEmpty = "";
            byte[] bytesNull = null;
            byte[] bytesEmpty = new byte[0];

            Hex hex = strNull;
            Assert.Equal(0, hex.Length);

            hex = strEmpty;
            Assert.Equal(0, hex.Length);

            hex = string.Empty;
            Assert.Equal(0, hex.Length);

            hex = bytesNull;
            Assert.Equal(0, hex.Length);

            hex = bytesEmpty;
            Assert.Equal(0, hex.Length);
        }


        [Fact]
        public void HexTypeBasicUsageTest()
        {
            const int repeats = 100000;

            // 1. Number<T> <-> Hex  변환 테스트
            // 2. Number<T> <-> Hex <-> byte[] 변환 테스트
            // 3. Number<T> <-> Hex <-> string 변환 테스트

            // bool ( true, false )
            for (int i = 0; i < 2; i++)
            {
                // bool <-> Hex
                bool expected = (i % 2 == 0);
                Hex hex = expected;
                bool actual = hex;
                Assert.Equal(expected, actual);

                // bool <-> Hex <-> byte[]
                byte[] bytes = hex;
                Hex hexBytes = bytes;
                actual = hexBytes;
                Assert.Equal(expected, actual);

                // bool <-> Hex <-> string
                string str = hex;
                Hex hexString = str;
                actual = hexString;
                Assert.Equal(expected, actual);
            }

            // byte ( 0 ~ 255 )
            for (int i = byte.MinValue; i <= byte.MaxValue; i++)
            {
                // byte <-> Hex
                byte expected = (byte)i;
                Hex hex = expected;
                byte actual = hex;
                Assert.Equal(expected, actual);

                // byte <-> Hex <-> byte[]
                byte[] bytes = hex;
                Hex hexBytes = bytes;
                actual = hexBytes;
                Assert.Equal(expected, actual);

                // byte <-> Hex <-> string
                string str = hex;
                Hex hexString = str;
                actual = hexString;
                Assert.Equal(expected, actual);
            }

            // sbyte ( -128 ~ 127 )
            for (int i = sbyte.MinValue; i <= sbyte.MaxValue; i++)
            {
                // sbyte <-> Hex
                sbyte expected = (sbyte)i;
                Hex hex = expected;
                sbyte actual = hex;
                Assert.Equal(expected, actual);

                // sbyte <-> Hex <-> byte[]
                byte[] bytes = hex;
                Hex hexBytes = bytes;
                actual = hexBytes;
                Assert.Equal(expected, actual);

                // sbyte <-> Hex <-> string
                string str = hex;
                Hex hexString = str;
                actual = hexString;
                Assert.Equal(expected, actual);
            }

            // short ( -32768 ~ 32767 )
            for (int i = short.MinValue; i <= short.MaxValue; i++)
            {
                // short <-> Hex
                short expected = (short)i;
                Hex hex = expected;
                short actual = hex;
                Assert.Equal(expected, actual);

                // short <-> Hex <-> byte[]
                byte[] bytes = hex;
                Hex hexBytes = bytes;
                actual = hexBytes;
                Assert.Equal(expected, actual);

                // short <-> Hex <-> string
                string str = hex;
                Hex hexString = str;
                actual = hexString;
                Assert.Equal(expected, actual);
            }

            // ushort ( 0 ~ 65535 )
            for (int i = ushort.MinValue; i <= ushort.MaxValue; i++)
            {
                // ushort <-> Hex
                ushort expected = (ushort)i;
                Hex hex = expected;
                ushort actual = hex;
                Assert.Equal(expected, actual);

                // ushort <-> Hex <-> byte[]
                byte[] bytes = hex;
                Hex hexBytes = bytes;
                actual = hexBytes;
                Assert.Equal(expected, actual);

                // ushort <-> Hex <-> string
                string str = hex;
                Hex hexString = str;
                actual = hexString;
                Assert.Equal(expected, actual);
            }

            // int 
            for (int i = 0; i < repeats; i++)
            {
                // int <-> Hex
                int expected = SecureRandom.Next<int>();
                Hex hex = expected;
                int actual = hex;
                Assert.Equal(expected, actual);

                // int <-> Hex <-> byte[]
                byte[] bytes = hex;
                Hex hexBytes = bytes;
                actual = hexBytes;
                Assert.Equal(expected, actual);

                // int <-> Hex <-> string
                string str = hex;
                Hex hexString = str;
                actual = hexString;
                Assert.Equal(expected, actual);
            }

            // uint
            for (int i = 0; i < repeats; i++)
            {
                // uint <-> Hex
                uint expected = SecureRandom.Next<uint>();
                Hex hex = expected;
                uint actual = hex;
                Assert.Equal(expected, actual);

                // uint <-> Hex <-> byte[]
                byte[] bytes = hex;
                Hex hexBytes = bytes;
                actual = hexBytes;
                Assert.Equal(expected, actual);

                // uint <-> Hex <-> string
                string str = hex;
                Hex hexString = str;
                actual = hexString;
                Assert.Equal(expected, actual);
            }

            // long
            for (int i = 0; i < repeats; i++)
            {
                // long <-> Hex
                long expected = SecureRandom.Next<long>();
                Hex hex = expected;
                long actual = hex;
                Assert.Equal(expected, actual);

                // long <-> Hex <-> byte[]
                byte[] bytes = hex;
                Hex hexBytes = bytes;
                actual = hexBytes;
                Assert.Equal(expected, actual);

                // long <-> Hex <-> string
                string str = hex;
                Hex hexString = str;
                actual = hexString;
                Assert.Equal(expected, actual);
            }

            // ulong
            for (int i = 0; i < repeats; i++)
            {
                // ulong <-> Hex
                ulong expected = SecureRandom.Next<ulong>();
                Hex hex = expected;
                ulong actual = hex;
                Assert.Equal(expected, actual);

                // ulong <-> Hex <-> byte[]
                byte[] bytes = hex;
                Hex hexBytes = bytes;
                actual = hexBytes;
                Assert.Equal(expected, actual);

                // ulong <-> Hex <-> string
                string str = hex;
                Hex hexString = str;
                actual = hexString;
                Assert.Equal(expected, actual);
            }

            // float
            for (int i = 0; i < repeats; i++)
            {
                // float <-> Hex
                float expected = SecureRandom.Next<float>();
                Hex hex = expected;
                float actual = hex;
                Assert.Equal(expected, actual);

                // float <-> Hex <-> byte[]
                byte[] bytes = hex;
                Hex hexBytes = bytes;
                actual = hexBytes;
                Assert.Equal(expected, actual);

                // float <-> Hex <-> string
                string str = hex;
                Hex hexString = str;
                actual = hexString;
                Assert.Equal(expected, actual);
            }

            // double
            for (int i = 0; i < repeats; i++)
            {
                // double <-> Hex
                double expected = SecureRandom.Next<double>();
                Hex hex = expected;
                double actual = hex;
                Assert.Equal(expected, actual);

                // double <-> Hex <-> byte[]
                byte[] bytes = hex;
                Hex hexBytes = bytes;
                actual = hexBytes;
                Assert.Equal(expected, actual);

                // double <-> Hex <-> string
                string str = hex;
                Hex hexString = str;
                actual = hexString;
                Assert.Equal(expected, actual);
            }

            // decimal
            for (int i = 0; i < repeats; i++)
            {
                // decimal <-> Hex
                decimal expected = SecureRandom.Next<decimal>();
                Hex hex = expected;
                decimal actual = hex;
                Assert.Equal(expected, actual);

                // decimal <-> Hex <-> byte[]
                byte[] bytes = hex;
                Hex hexBytes = bytes;
                actual = hexBytes;
                Assert.Equal(expected, actual);

                // decimal <-> Hex <-> string
                string str = hex;
                Hex hexString = str;
                actual = hexString;
                Assert.Equal(expected, actual);
            }
        }


        public int HexStringCompare(string left, string right)
        {
            string InvalidHexExp = @"[^\dabcdef]";
            string HexPaddingExp = @"^(0x)?0*";
            //Remove whitespace, "0x" prefix if present, and leading zeros.  
            //Also make all characters lower case.
            string l = Regex.Replace(left.Trim().ToLower(), HexPaddingExp, "");
            string r = Regex.Replace(right.Trim().ToLower(), HexPaddingExp, "");

            //validate that values contain only hex characters
            if (Regex.IsMatch(l, InvalidHexExp))
            {
                throw new ArgumentOutOfRangeException("Value1 is not a hex string");
            }
            if (Regex.IsMatch(r, InvalidHexExp))
            {
                throw new ArgumentOutOfRangeException("Value2 is not a hex string");
            }

            int Result = l.Length.CompareTo(r.Length);
            if (Result == 0)
                Result = l.CompareTo(r);

            return Result;
        }

        [Fact]
        public void Check()
        {
            string left = "0x000012";
            string right = "0x0012";

            byte[] lbytes = Hex.ToByteArray(left);
            byte[] rbytes = Hex.ToByteArray(right);

            Hex l = left;
            Hex r = right;

            Assert.True(l.Equals(lbytes));
            Assert.True(r.Equals(rbytes));

            // equals() 비교는 false
            Assert.False(l.Equals(r));

            // == 비교는 true (값이 동일)
            Assert.True(l == r);
            Assert.False(l != r);

            // 크기 비교
            Hex h = "0x10";
            Assert.True(r > h);
            Assert.True(r >= h);
            Assert.True(r != h);
            Assert.False(r < h);
            Assert.False(r <= h);
            Assert.False(r == h);
        }

        [Fact]
        public void HexShouldComparable()
        {
            const int repeats = 100000;

            for (int i = 0; i < repeats; i++)
            {
                byte[] bytes1 = SecureRandom.GetBytes(32);
                byte[] bytes2 = SecureRandom.GetBytes(32);

                Hex h1 = bytes1;
                Hex h2 = bytes2;

                string s1 = string.Concat(bytes1.Select(b => b.ToString("x2")).ToArray());
                string s2 = string.Concat(bytes2.Select(b => b.ToString("x2")).ToArray());

                Assert.Equal(s1.CompareTo(s2), h1.CompareTo(h2));

                if (s1.CompareTo(s2) > 0)
                {
                    Assert.True(h1 > h2);
                    Assert.True(h1 >= h2);
                    Assert.False(h1 < h2);
                    Assert.False(h1 <= h2);
                    Assert.False(h1 == h2);
                    Assert.True(h1 != h2);
                }
                else if (s1.CompareTo(s2) != 0)
                {
                    Assert.False(h1 > h2);
                    Assert.False(h1 >= h2);
                    Assert.True(h1 < h2);
                    Assert.True(h1 <= h2);
                    Assert.False(h1 == h2);
                    Assert.True(h1 != h2);
                }
            }
        }

        [Theory]
        [InlineData(100000)]
        public void HexShouldBeDictionaryKey(int repeats)
        {
            var dicts = new Dictionary<Hex, byte[]>();

            for (int i = 0; i < repeats; i++)
            {
                var key = SecureRandom.Next<long>();
                var value = SecureRandom.GetBytes(32);

                dicts[key] = value;
                Assert.Equal(value, dicts[key]);
            }
        }
    }
}
