using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bryllite.Cryptography.Hash
{
    public class Sha3Provider
    {
        public Sha3Provider()
        {
        }

        public byte[] Hash(byte[] message, int bits)
        {
            switch (bits)
            {
                case 128: return Hash128(message);
                case 160: return Hash160(message);
                case 256: return Hash256(message);
                case 384: return Hash384(message);
                case 512: return Hash512(message);
                default: break;
            }

            throw new ArgumentException("unsupported bit length!");
        }

        public byte[] H128(byte[] message)
        {
            return Hash128(message);
        }

        public byte[] H160(byte[] message)
        {
            return Hash160(message);
        }

        public byte[] H256(byte[] message)
        {
            return Hash256(message);
        }

        public byte[] H384(byte[] message)
        {
            return Hash384(message);
        }

        public byte[] H512(byte[] message)
        {
            return Hash512(message);
        }

        public static byte[] Hash128(byte[] message)
        {
            return ComputeHash(message, 128);
        }

        public static byte[] Hash160(byte[] message)
        {
            return Hash256(message).Skip(12).ToArray();
        }

        public static byte[] Hash256(byte[] message)
        {
            return ComputeHash(message, 256);
        }

        public static byte[] Hash384(byte[] message)
        {
            return ComputeHash(message, 384);
        }

        public static byte[] Hash512(byte[] message)
        {
            return ComputeHash(message, 512);
        }

        protected static byte[] ComputeHash(byte[] bytes, int bits)
        {
            var digest = new Sha3Digest(bits);
            var output = new byte[digest.GetDigestSize()];

            byte[] message = bytes ?? new byte[0];
            digest.BlockUpdate(message, 0, message.Length);
            digest.DoFinal(output, 0);

            return output;
        }

    }
}
