using Bryllite.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bryllite.Cryptography.Hash.Extensions
{
    public static class HashExtension
    {
        // default hash provider == keccak
        private static KeccakProvider hp = new KeccakProvider();

        public static byte[] Hash(this byte[] message, int bits)
        {
            return hp.Hash(message, bits);
        }

        public static byte[] Hash(this string message, int bits)
        {
            byte[] data = Hex.IsHexString(message) ? Hex.ToByteArray(message) : Encoding.UTF8.GetBytes(message);
            return Hash(data, bits);
        }

        public static byte[] Hash128(this byte[] message)
        {
            return Hash(message, 128);
        }

        public static byte[] Hash160(this byte[] message)
        {
            return Hash(message, 160);
        }

        public static byte[] Hash256(this byte[] message)
        {
            return Hash(message, 256);
        }

        public static byte[] Hash384(this byte[] message)
        {
            return Hash(message, 384);
        }

        public static byte[] Hash512(this byte[] message)
        {
            return Hash(message, 512);
        }

        public static byte[] Hash128(this string message)
        {
            return Hash(message, 128);
        }

        public static byte[] Hash160(this string message)
        {
            return Hash(message, 160);
        }

        public static byte[] Hash256(this string message)
        {
            return Hash(message, 256);
        }

        public static byte[] Hash384(this string message)
        {
            return Hash(message, 384);
        }

        public static byte[] Hash512(this string message)
        {
            return Hash(message, 512);
        }

        public static byte[] Hash128(this byte[] message, byte[] iv)
        {
            return Hash128(message.Append(iv));
        }

        public static byte[] Hash160(this byte[] message, byte[] iv)
        {
            return Hash160(message.Append(iv));
        }

        public static byte[] Hash256(this byte[] message, byte[] iv)
        {
            return Hash256(message.Append(iv));
        }

        public static byte[] Hash384(this byte[] message, byte[] iv)
        {
            return Hash384(message.Append(iv));
        }

        public static byte[] Hash512(this byte[] message, byte[] iv)
        {
            return Hash512(message.Append(iv));
        }

        public static bool HashEquals(this byte[] left, byte[] right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
            if (left.Length != right.Length) return false;

            return left.Hash256().SequenceEqual(right.Hash256());
        }
    }
}
