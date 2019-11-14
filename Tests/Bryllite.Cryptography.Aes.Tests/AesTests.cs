using System;
using System.Text;
using Xunit;

namespace Bryllite.Cryptography.Aes.Tests
{
    public class AesTests
    {
        const int repeats = 100000;

        [Fact]
        public void Aes256ShouldDecryptable()
        {
            for (int i = 0; i < repeats; i++)
            {
                // random key ( 0 - 256 )
                byte[] key = SecureRandom.GetBytes(SecureRandom.Next(0, 256));

                // random iv ( 0 - 256 )
                byte[] iv = SecureRandom.GetBytes(SecureRandom.Next(0, 256));

                // random message ( 0 - 1024 )
                byte[] expected = SecureRandom.GetBytes(SecureRandom.Next(0, 1024));

                // Encrypt / Decrypt
                Assert.Equal(expected, new Aes256(key).Decrypt(new Aes256(key).Encrypt(expected)));
                Assert.Equal(expected, new Aes256(key, iv).Decrypt(new Aes256(key, iv).Encrypt(expected)));

                // static Encrypt / Decrypt
                Assert.Equal(expected, Aes256.Decrypt(key, Aes256.Encrypt(key, expected)));
                Assert.Equal(expected, Aes256.Decrypt(key, iv, Aes256.Encrypt(key, iv, expected)));

                // TryEncrypt / TryDecrypt without iv
                {
                    Assert.True(Aes256.TryEncrypt(key, expected, out var encrypted));
                    Assert.True(Aes256.TryDecrypt(key, encrypted, out var actual));
                    Assert.Equal(expected, actual);
                }

                // TryEncrypt / TryDecrypt with iv
                {
                    Assert.True(Aes256.TryEncrypt(key, iv, expected, out var encrypted));
                    Assert.True(Aes256.TryDecrypt(key, iv, encrypted, out var actual));
                    Assert.Equal(expected, actual);
                }
            }
        }

        [Fact]
        public void Aes128ShouldDecryptable()
        {
            for (int i = 0; i < repeats; i++)
            {
                // random key ( 0 - 256 )
                byte[] key = SecureRandom.GetBytes(SecureRandom.Next(0, 256));

                // random iv ( 0 - 256 )
                byte[] iv = SecureRandom.GetBytes(SecureRandom.Next(0, 256));

                // random message ( 0 - 1024 )
                byte[] expected = SecureRandom.GetBytes(SecureRandom.Next(0, 1024));

                // Encrypt / Decrypt
                Assert.Equal(expected, new Aes128(key).Decrypt(new Aes128(key).Encrypt(expected)));
                Assert.Equal(expected, new Aes128(key, iv).Decrypt(new Aes128(key, iv).Encrypt(expected)));

                // static Encrypt / Decrypt
                Assert.Equal(expected, Aes128.Decrypt(key, Aes128.Encrypt(key, expected)));
                Assert.Equal(expected, Aes128.Decrypt(key, iv, Aes128.Encrypt(key, iv, expected)));

                // TryEncrypt / TryDecrypt without iv
                {
                    Assert.True(Aes128.TryEncrypt(key, expected, out var encrypted));
                    Assert.True(Aes128.TryDecrypt(key, encrypted, out var actual));
                    Assert.Equal(expected, actual);
                }

                // TryEncrypt / TryDecrypt with iv
                {
                    Assert.True(Aes128.TryEncrypt(key, iv, expected, out var encrypted));
                    Assert.True(Aes128.TryDecrypt(key, iv, encrypted, out var actual));
                    Assert.Equal(expected, actual);
                }
            }
        }
    }
}
