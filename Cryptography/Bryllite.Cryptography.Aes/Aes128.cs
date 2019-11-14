using Bryllite.Cryptography.Hash;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Bryllite.Cryptography.Aes
{
    public class Aes128
    {
        // key size
        public const int KEY_SIZE = 128;

        // encrypt/decrypt key
        private readonly byte[] key;

        // iv
        private readonly byte[] iv;

        // cipher mode
        public CipherMode Mode = CipherMode.CBC;

        // padding mode
        public PaddingMode Padding = PaddingMode.PKCS7;

        public Aes128(byte[] key, byte[] iv)
        {
            this.key = KeccakProvider.Hash128(key);
            this.iv = KeccakProvider.Hash128(iv);
        }

        public Aes128(byte[] key) : this(key, key)
        {
        }

        public byte[] Encrypt(byte[] plain)
        {
            try
            {
                RijndaelManaged rm = new RijndaelManaged();
                rm.Mode = Mode;
                rm.Padding = Padding;
                rm.KeySize = KEY_SIZE;
                rm.Key = key;
                rm.IV = iv;

                using (var encryptor = rm.CreateEncryptor())
                {
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(plain, 0, plain.Length);
                            cs.FlushFinalBlock();

                            return ms.ToArray();
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public byte[] Decrypt(byte[] encrypted)
        {
            try
            {
                RijndaelManaged rm = new RijndaelManaged();
                rm.Mode = Mode;
                rm.Padding = Padding;
                rm.KeySize = KEY_SIZE;
                rm.Key = key;
                rm.IV = iv;

                using (var encryptor = rm.CreateDecryptor())
                {
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(encrypted, 0, encrypted.Length);
                            cs.FlushFinalBlock();

                            return ms.ToArray();
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }


        public static byte[] Encrypt(byte[] key, byte[] iv, byte[] plain)
        {
            return new Aes128(key, iv).Encrypt(plain);
        }

        public static byte[] Encrypt(byte[] key, byte[] plain)
        {
            return new Aes128(key).Encrypt(plain);
        }

        public static byte[] Decrypt(byte[] key, byte[] iv, byte[] encrypted)
        {
            return new Aes128(key, iv).Decrypt(encrypted);
        }

        public static byte[] Decrypt(byte[] key, byte[] encrypted)
        {
            return new Aes128(key).Decrypt(encrypted);
        }

        public static bool TryEncrypt(byte[] key, byte[] iv, byte[] plain, out byte[] encrypted)
        {
            try
            {
                encrypted = Encrypt(key, iv, plain);
                return encrypted.Length > 0;
            }
            catch
            {
                encrypted = null;
                return false;
            }
        }

        public static bool TryEncrypt(byte[] key, byte[] plain, out byte[] encrypted)
        {
            try
            {
                encrypted = Encrypt(key, plain);
                return encrypted.Length > 0;
            }
            catch
            {
                encrypted = null;
                return false;
            }
        }

        public static bool TryDecrypt(byte[] key, byte[] iv, byte[] encrypted, out byte[] plain)
        {
            try
            {
                plain = Decrypt(key, iv, encrypted);
                return plain.Length > 0;
            }
            catch
            {
                plain = null;
                return false;
            }
        }

        public static bool TryDecrypt(byte[] key, byte[] encrypted, out byte[] plain)
        {
            try
            {
                plain = Decrypt(key, encrypted);
                return plain.Length > 0;
            }
            catch
            {
                plain = null;
                return false;
            }
        }

    }
}
