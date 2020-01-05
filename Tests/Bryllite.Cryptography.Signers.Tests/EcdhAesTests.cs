using Bryllite.Cryptography.Aes;
using Bryllite.Cryptography.Hash;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Bryllite.Cryptography.Signers.Tests
{
    public class EcdhAesTests
    {
        [Fact]
        public void EcdhAesSanityTest()
        {
            PrivateKey left = "0x71807c6849611ea301bd79e53e73bc43835ba7c12c5a819014e8b1d0f575b3a4";
            PrivateKey right = "0xc7ed2a22fd193cc38a465a983d4ff21c41c53d6d35d85d83629ae60a16e300f8";
            PrivateKey expected = "0x1549c8e0c11b556dafd8f0355ef9def2a13c9895dded3018380d37b639b7e688";

            Assert.Equal(expected, left.CreateEcdhKey(right.PublicKey));
            Assert.Equal(expected, right.CreateEcdhKey(left.PublicKey));

            byte[] message = Encoding.UTF8.GetBytes("Hello, Bryllite!");
            byte[] encrypted = Aes256.Encrypt(expected, message);

            Assert.Equal("0x52ccf8b53077a2d710754acea1db43a2f098cd8718e6b69fe0a0223036928776", Hex.ToString(encrypted));

            byte[] actual = Aes256.Decrypt(expected, encrypted);
            Assert.Equal(message, actual);
        }

        [Fact]
        public void EcdhSanityCheck()
        {
            PrivateKey k1 = "0x82fc9947e878fc7ed01c6c310688603f0a41c8e8704e5b990e8388343b0fd465";
            PrivateKey k2 = "0x5f706787ac72c1080275c1f398640fb07e9da0b124ae9734b28b8d0f01eda586";

            Assert.Equal("0x04c7c674a223661faefed8515febacc411c0f3569c10c65ec86cdce4d85c7ea26c617f0cf19ce0b10686501e57af1a7002282fefa52845be2267d1f4d7af322974",
                Hex.ToString(k1.PublicKey.UncompressedKey));

            Assert.Equal("0x04b80cdf1422644ccfb0a2c73103bdfa3cc96786c3e63d8df70267fc7fffe711a1000d37a20cefd1fdcceec0b0b3f25a46c8a430800ba0c19f4ae0cfc582de8fb8",
                Hex.ToString(k2.PublicKey.UncompressedKey));

            PrivateKey shared = k1.CreateEcdhKey(k2.PublicKey);

            Assert.Equal("0x5935d0476af9df2998efb60383adf2ff23bc928322cfbb738fca88e49d557d7e", (string)shared);

            byte[] hashed = Sha2Provider.Hash256(Hex.ToByteArray("0x033a17fe5fa33c4f2c7e61799a65061214913f39bfcbee178ab351493d5ee17b2f"));
            Assert.Equal("0x5935d0476af9df2998efb60383adf2ff23bc928322cfbb738fca88e49d557d7e", Hex.ToString(hashed));
        }
    }
}
