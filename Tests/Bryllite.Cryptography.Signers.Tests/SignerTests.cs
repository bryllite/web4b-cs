using System;
using Xunit;

namespace Bryllite.Cryptography.Signers.Tests
{
    public class SignerTests
    {
        const int repeats = 100000;

        [Fact]
        public void SignerShouldGenerateValidKey()
        {
            for (int i = 0; i < repeats; i++)
            {
                H256 messageHash = SecureRandom.GetBytes(H256.BYTE_LENGTH);

                PrivateKey secKey = PrivateKey.CreateKey();
                PublicKey pubKey = secKey.PublicKey;
                Signature sig = secKey.Sign(messageHash);

                // private key verify
                Assert.True(Secp256k1Helper.SecretKeyVerify(secKey.Key));

                // private key, public key, signature length verify
                Assert.Equal(PrivateKey.KEY_LENGTH, secKey.Key.Length);
                Assert.Equal(PublicKey.KEY_LENGTH, pubKey.Bytes.Length);
                Assert.Equal(Signature.BYTE_LENGTH, sig.Bytes.Length);

                // verify signature
                Assert.True(pubKey.Verify(sig, messageHash));
                Assert.Equal(pubKey, sig.GetPublicKey(messageHash));
            }
        }

        [Fact]
        public void SignerShouldRecoverableWithValidRecoverId()
        {
            for (int i = 0; i < repeats; i++)
            {
                H256 messageHash = SecureRandom.GetBytes(H256.BYTE_LENGTH);

                PrivateKey privateKey = PrivateKey.CreateKey();

                Signature sig = privateKey.Sign(messageHash);

                Assert.True(sig.V < 4);
                Assert.Equal(privateKey.PublicKey, sig.GetPublicKey(messageHash));

                byte[] bytes = sig;
                bytes[64] = 99;

                Signature wrong = Signature.Parse(bytes);
                Assert.NotEqual(privateKey.PublicKey, wrong.GetPublicKey(messageHash));
            }
        }

        [Fact]
        public void SignerShouldBeDeterministic()
        {
            for (int i = 0; i < repeats; i++)
            {
                H256 messageHash = SecureRandom.GetBytes(H256.BYTE_LENGTH);

                PrivateKey privateKey = PrivateKey.CreateKey();

                Signature sig1 = privateKey.Sign(messageHash);
                Signature sig2 = privateKey.Sign(messageHash);

                Assert.Equal(sig1, sig2);
            }
        }

        [Fact]
        public void RecoverSanity()
        {
            H256 messageHash = "0xce7df6b1b2852c5c156b683a9f8d4a8daeda2f35f025cb0cf34943dcac70d6a3";
            PrivateKey key = "0x97ddae0f3a25b92268175400149d65d6887b9cefaf28ea2c078e05cdc15a3c0a";

            string sPublicKey = "0x7b83ad6afb1209f3c82ebeb08c0c5fa9bf6724548506f2fb4f991e2287a77090177316ca82b0bdf70cd9dee145c3002c0da1d92626449875972a27807b73b42e";
            string compressedKey = "0x027b83ad6afb1209f3c82ebeb08c0c5fa9bf6724548506f2fb4f991e2287a77090";
            string expectedR = "0x6f0156091cbe912f2d5d1215cc3cd81c0963c8839b93af60e0921b61a19c5430";
            string expectedS = "0x0c71006dd93f3508c432daca21db0095f4b16542782b7986f48a5d0ae3c583d4";
            byte expectedV = 1;

            Assert.Equal(sPublicKey, Hex.ToString(key.PublicKey.Key));
            Assert.Equal(compressedKey, Hex.ToString(key.PublicKey.CompressedKey));

            Signature sig = key.Sign(messageHash);

            Assert.Equal(expectedR, Hex.ToString(sig.R));
            Assert.Equal(expectedS, Hex.ToString(sig.S));
            Assert.Equal(expectedV, sig.V);

            PublicKey recovered = sig.GetPublicKey(messageHash);

            Assert.Equal(sPublicKey, Hex.ToString(recovered.Key));
            Assert.True(recovered.Verify(sig, messageHash));
        }

        [Fact]
        public void EcdhTest()
        {
            for (int i = 0; i < repeats; i++)
            {
                PrivateKey a = PrivateKey.CreateKey();
                PrivateKey b = PrivateKey.CreateKey();

                string aa = a.PublicKey;
                string bb = b.PublicKey;

                var expected = a.CreateSharedKey(bb);
                var actual = b.CreateSharedKey(aa);

                Assert.Equal(expected, actual);
            }
        }
    }
}
