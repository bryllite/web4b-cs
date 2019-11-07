using System;
using SECP256K1 = Secp256k1Net.Secp256k1;

namespace Secp256k1.Net
{
    /// <summary>
    /// SECP256K1.NET wrapper class
    /// </summary>
    public class Secp256k1Wrapper
    {
        // secp256k1.net
        private static readonly SECP256K1 secp256k1 = new SECP256K1();

        public const int PRIVATE_KEY_LENGTH = SECP256K1.PRIVKEY_LENGTH;
        public const int PUBLIC_KEY_LENGTH = SECP256K1.PUBKEY_LENGTH;
        public const int SIGNATURE_LENGTH = SECP256K1.SIGNATURE_LENGTH;
        public const int SERIALIZED_UNCOMPRESSED_PUBKEY_LENGTH = SECP256K1.SERIALIZED_UNCOMPRESSED_PUBKEY_LENGTH;
        public const int SERIALIZED_COMPRESSED_PUBKEY_LENGTH = SECP256K1.SERIALIZED_COMPRESSED_PUBKEY_LENGTH;

        private Secp256k1Wrapper()
        {
        }

        public static bool SecretKeyVerify(byte[] secretKey)
        {
            try
            {
                lock (secp256k1)
                    return secp256k1.SecretKeyVerify(secretKey);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static byte[] PublicKeyCreate(byte[] secretKey)
        {
            byte[] publicKey = new byte[PUBLIC_KEY_LENGTH];
            lock (secp256k1)
                return secp256k1.PublicKeyCreate(publicKey, secretKey) ? publicKey : null;
        }

        public static byte[] PublicKeySerialize(byte[] pubKey)
        {
            return PublicKeySerialize(pubKey, false);
        }

        public static byte[] PublicKeySerialize(byte[] pubKey, bool compressed)
        {
            int len = compressed ? SECP256K1.SERIALIZED_COMPRESSED_PUBKEY_LENGTH : SECP256K1.SERIALIZED_UNCOMPRESSED_PUBKEY_LENGTH;
            byte[] bytes = new byte[len];
            lock (secp256k1)
                return secp256k1.PublicKeySerialize(bytes, pubKey, compressed ? Secp256k1Net.Flags.SECP256K1_EC_COMPRESSED : Secp256k1Net.Flags.SECP256K1_EC_UNCOMPRESSED) ? bytes : null;
        }

        public static byte[] PublicKeyParse(byte[] bytes)
        {
            byte[] pubKey = new byte[PUBLIC_KEY_LENGTH];
            lock (secp256k1)
                return secp256k1.PublicKeyParse(pubKey, bytes) ? pubKey : null;
        }

        public static byte[] Sign(byte[] messageHash, byte[] secretKey)
        {
            byte[] signature = new byte[SIGNATURE_LENGTH];
            lock (secp256k1)
                return secp256k1.Sign(signature, messageHash, secretKey) ? signature : null;
        }

        public static byte[] SignRecoverable(byte[] messageHash, byte[] secretKey)
        {
            byte[] signature = new byte[SIGNATURE_LENGTH + 1];
            lock (secp256k1)
                return secp256k1.SignRecoverable(signature, messageHash, secretKey) ? signature : null;
        }

        public static bool Verify(byte[] signature, byte[] messageHash, byte[] pubKey)
        {
            try
            {
                lock (secp256k1)
                    return secp256k1.Verify(signature, messageHash, pubKey);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static byte[] Recover(byte[] signature, byte[] messageHash)
        {
            byte[] pubKey = new byte[PUBLIC_KEY_LENGTH];
            lock (secp256k1)
                return secp256k1.Recover(pubKey, signature, messageHash) ? pubKey : null;
        }

        public static byte[] CreateSharedSecretKey(byte[] privateKey, byte[] publicKey)
        {
            byte[] secretKey = new byte[PRIVATE_KEY_LENGTH];
            lock (secp256k1)
                return secp256k1.Ecdh(secretKey, publicKey, privateKey) ? secretKey : null;
        }
    }
}
