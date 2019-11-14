using System;
using System.Text;
using Xunit;

namespace Bryllite.Cryptography.Hash.Tests
{
    public class HashTests
    {
        [Fact]
        public void HashProviderShouldHashNullOrEmpty()
        {
            // sha2
            Hex sha2 = Sha2Provider.Hash256(null);
            Assert.True(sha2 == "0xe3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855");
            Assert.True(sha2 == Sha2Provider.Hash256(new byte[0]));
            Assert.True(sha2 == Sha2Provider.Hash256(Encoding.UTF8.GetBytes("")));

            // sha3
            Hex sha3 = Sha3Provider.Hash256(null);
            Assert.True(sha3 == "0xa7ffc6f8bf1ed76651c14756a061d662f580ff4de43b49fa82d80a4b80f8434a");
            Assert.True(sha3 == Sha3Provider.Hash256(new byte[0]));

            // keccak
            Hex keccak = KeccakProvider.Hash256(null);
            Assert.True(keccak == "0xc5d2460186f7233c927e7db2dcc703c0e500b653ca82273b7bfad8045d85a470");
            Assert.True(keccak == KeccakProvider.Hash256(new byte[0]));

            // blake2s
            Hex blake2s = Blake2sProvider.Hash256(null);
            Assert.True(blake2s == "0x69217a3079908094e11121d042354a7c1f55b6482ca1a51e1b250dfd1ed0eef9");
            Assert.True(blake2s == Blake2sProvider.Hash256(new byte[0]));

            // blake2b
            Hex blake2b = Blake2bProvider.Hash512(null);
            Assert.True(blake2b == "0x786a02f742015903c6c6fd852552d272912f4740e15847618a86e217f71f5419d25e1031afee585313896444934eb04b903a685b1448b755d56f701afe9be2ce");
            Assert.True(blake2b == Blake2bProvider.Hash512(new byte[0]));

            // ripemd
            Hex ripemd = RipemdProvider.Hash160(null);
            Assert.True(ripemd == "0x9c1185a5c5e9fc54612808977ee8f548b2258d31");
            Assert.True(ripemd == RipemdProvider.Hash160(new byte[0]));


            blake2b = Blake2bProvider.Hash512(Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog"));
            Assert.True(blake2b == "0xa8add4bdddfd93e4877d2746e62817b116364a1fa7bc148d95090bc7333b3673f82401cf7aa2e4cb1ecd90296e3f14cb5413f8ed77be73045b13914cdcd6a918");
            blake2b = Blake2bProvider.Hash512(Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dof"));
            Assert.True(blake2b == "0xab6b007747d8068c02e25a6008db8a77c218d94f3b40d2291a7dc8a62090a744c082ea27af01521a102e42f480a31e9844053f456b4b41e8aa78bbe5c12957bb");
        }
    }
}
