using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Bryllite.Cryptography.Hash
{
    public class HmacSha2
    {
        public static byte[] H256(byte[] key, byte[] message)
        {
            using (var digest = new HMACSHA256(key))
            {
                return digest.ComputeHash(message);
            }
        }

        public static byte[] H512(byte[] key, byte[] message)
        {
            using (var digest = new HMACSHA512(key))
            {
                return digest.ComputeHash(message);
            }
        }
    }
}
