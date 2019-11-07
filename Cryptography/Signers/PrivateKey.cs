using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Bryllite.Cryptography.Hash;
using Bryllite.Extensions;
using Secp256k1.Net;

namespace Bryllite.Cryptography.Signers
{
    public class PrivateKey
    {
        public static readonly int KEY_LENGTH = Secp256k1Wrapper.PRIVATE_KEY_LENGTH;
        public static readonly int CHAIN_CODE_LENGTH = KEY_LENGTH;

        // null key
        public static readonly PrivateKey Null = new PrivateKey();

        // private key
        public H256 Key { get; private set; }

        // chain code
        public H256 ChainCode { get; private set; }

        // key hex ( 64 or 128 length )
        public string Hex => ToByteArray().ToHexString();

        // public key
        public PublicKey PublicKey => ToPublicKey();

        // EOA Address
        public Address Address => PublicKey.Address;

        protected PrivateKey()
        {
        }

        public PrivateKey(H256 key) : this()
        {
            Key = key;
        }

        public PrivateKey(H256 key, H256 chainCode) : this(key)
        {
            ChainCode = chainCode;
        }

        public PrivateKey(byte[] bytes) : this()
        {
            if (bytes.IsNullOrEmpty()) throw new ArgumentNullException("empty key bytes");

            if (bytes.Length == KEY_LENGTH)
            {
                Key = bytes;
            }
            else if (bytes.Length == KEY_LENGTH + CHAIN_CODE_LENGTH)
            {
                Key = bytes.Slice(0, KEY_LENGTH);
                ChainCode = bytes.Slice(KEY_LENGTH, CHAIN_CODE_LENGTH);
            }
            else throw new ArgumentException("invalid key bytes");
        }

        public PrivateKey(string hex) : this(hex.ToByteArray())
        {
        }

        public PrivateKey(string hex, string chaincode) : this(hex.ToByteArray(), chaincode.ToByteArray())
        {
        }

        public byte[] ToByteArray()
        {
            return Key.Concat((byte[])ChainCode);
        }

        public static PrivateKey Parse(byte[] bytes)
        {
            return new PrivateKey(bytes);
        }

        public static PrivateKey Parse(string hex)
        {
            return new PrivateKey(hex);
        }

        public static bool TryParse(byte[] bytes, out PrivateKey key)
        {
            try
            {
                key = Parse(bytes);
                return true;
            }
            catch (Exception)
            {
                key = null;
                return false;
            }
        }

        public static bool TryParse(string hex, out PrivateKey key)
        {
            try
            {
                key = Parse(hex);
                return true;
            }
            catch (Exception)
            {
                key = null;
                return false;
            }
        }


        public PrivateKey Clone()
        {
            return new PrivateKey(Key, ChainCode);
        }

        public PrivateKey CreateSharedKey(PublicKey key)
        {
            return new PrivateKey(Secp256k1Wrapper.CreateSharedSecretKey(Key, key.Bytes));
        }

        public static PrivateKey CreateKey()
        {
            return CreateKey(SecureRandom.GetBytes(KEY_LENGTH));
        }

        public static PrivateKey CreateKey(byte[] seed)
        {
            H256 key = seed.IsNullOrEmpty() ? SecureRandom.GetBytes(KEY_LENGTH) : seed.Hash256();
            return CheckPrivateKey(key) ? new PrivateKey(key) : CreateKey(key);
        }

        public static bool CheckPrivateKey(H256 key)
        {
            return Secp256k1Wrapper.SecretKeyVerify(key);
        }

        public override string ToString()
        {
            return Hex;
        }

        public override int GetHashCode()
        {
            return new BigInteger(ToByteArray()).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as PrivateKey;
            return !ReferenceEquals(o, null) && Key == o.Key && ChainCode == o.ChainCode;
        }

        public static bool operator ==(PrivateKey left, PrivateKey right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(PrivateKey left, PrivateKey right)
        {
            return !(left == right);
        }

        public PrivateKey CKD(byte[] seed)
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(Key);
            bytes.AddRange(ChainCode ?? Key.ToByteArray().Hash256());
            bytes.AddRange(seed);

            (H256 key, H256 chainCode) = bytes.ToArray().Hash512().Divide();

            return CheckPrivateKey(key) ? new PrivateKey(key, chainCode) : CKD((byte[])key);
        }

        public PrivateKey CKD(string keyPath)
        {
            return CKD(keyPath.Split('/'));
        }

        public PrivateKey CKD(string[] keyPath)
        {
            if (keyPath.IsNullOrEmpty()) throw new ArgumentNullException("empty keyPath");

            PrivateKey key = Clone();
            foreach (var path in keyPath)
                key = key.CKD(Encoding.UTF8.GetBytes(path));

            return key;
        }

        public PublicKey ToPublicKey()
        {
            return PublicKey.Parse(Secp256k1Wrapper.PublicKeyCreate(Key));
        }

        public Signature Sign(byte[] messageHash)
        {
            Guard.Assert(!messageHash.IsNullOrEmpty() && messageHash.Length == 32, "messageHash length shoud be 32 bytes");
            return Signature.Parse(Secp256k1Wrapper.SignRecoverable(messageHash, Key));
        }

        public static implicit operator PrivateKey(byte[] bytes)
        {
            return TryParse(bytes, out PrivateKey key) ? key : null;
        }

        public static implicit operator PrivateKey(string hex)
        {
            return TryParse(hex, out PrivateKey key) ? key : null;
        }

        public static implicit operator byte[](PrivateKey key)
        {
            return key?.ToByteArray();
        }

        public static implicit operator string(PrivateKey key)
        {
            return key?.Hex;
        }
    }
}
