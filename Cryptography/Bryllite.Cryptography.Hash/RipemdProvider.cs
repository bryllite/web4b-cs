using System;
using Bryllite.Extensions;
using Org.BouncyCastle.Crypto.Digests;

namespace Bryllite.Cryptography.Hash
{
    public class RipemdProvider
    {
        public RipemdProvider()
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
                default:
                    break;
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
            var digest = new RipeMD128Digest();
            var output = new byte[digest.GetDigestSize()];

            byte[] msg = message ?? new byte[0];
            digest.BlockUpdate(msg, 0, msg.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        public static byte[] Hash160(byte[] message)
        {
            var digest = new RipeMD160Digest();
            var output = new byte[digest.GetDigestSize()];

            byte[] msg = message ?? new byte[0];
            digest.BlockUpdate(msg, 0, msg.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        public static byte[] Hash256(byte[] message)
        {
            var digest = new RipeMD256Digest();
            var output = new byte[digest.GetDigestSize()];

            byte[] msg = message ?? new byte[0];
            digest.BlockUpdate(msg, 0, msg.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        public static byte[] Hash384(byte[] message)
        {
            byte[] hash = Hash256(message);
            return hash.Append(Hash128(hash));
        }

        public static byte[] Hash512(byte[] message)
        {
            byte[] hash = Hash256(message);
            return hash.Append(Hash256(hash));
        }

    }
}
