using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Bryllite.Cryptography.Hash;
using Bryllite.Cryptography.Hash.Extensions;
using Bryllite.Extensions;

namespace Bryllite.Cryptography.Signers
{
    public class PrivateKey : Hex
    {
        public const int KEY_LENGTH = Secp256k1Helper.PRIVATE_KEY_LENGTH;
        public const int CHAIN_CODE_LENGTH = H256.BYTE_LENGTH;

        // private key
        public H256 Key
        {
            get { return Value.Take(KEY_LENGTH).ToArray(); }
        }

        // chain code
        public H256 ChainCode
        {
            get { return Length >= KEY_LENGTH + CHAIN_CODE_LENGTH ? Value.Skip(KEY_LENGTH).Take(CHAIN_CODE_LENGTH).ToArray() : null; }
        }

        // public key
        public PublicKey PublicKey
        {
            get { return PublicKey.Parse(Secp256k1Helper.PublicKeyCreate(Key)); }
        }

        // address
        public Address Address
        {
            get { return PublicKey.Address; }
        }

        public PrivateKey(Hex key) : base(key)
        {
            Guard.Assert(Length == KEY_LENGTH || Length == KEY_LENGTH + CHAIN_CODE_LENGTH, "wrong key bytes");
        }

        public static new PrivateKey Parse(byte[] bytes)
        {
            return new PrivateKey(bytes);
        }

        public static new PrivateKey Parse(string hex)
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
            catch
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
            catch
            {
                key = null;
                return false;
            }
        }

        public new PrivateKey Clone()
        {
            return new PrivateKey(this);
        }

        public PrivateKey CreateEcdhKey(PublicKey key)
        {
            return new PrivateKey(Secp256k1Helper.CreateEcdhKey(Key, key));
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
            return Secp256k1Helper.SecretKeyVerify(key);
        }

        public PrivateKey CKD(byte[] seed)
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(Key);
            bytes.AddRange(ChainCode ?? Key.ToByteArray().Hash256());
            bytes.AddRange(seed);

            byte[] h512 = KeccakProvider.Hash512(bytes.ToArray());
            (H256 key, H256 chaincode) = h512.Divide();
            return CheckPrivateKey(key) ? new PrivateKey(h512) : CKD((byte[])key);
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

        public Signature Sign(byte[] messageHash)
        {
            Guard.Assert(messageHash.Length == 32, "messageHash length shoud be 32 bytes");
            return Signature.Parse(Secp256k1Helper.SignRecoverable(messageHash, Key));
        }

        public static implicit operator PrivateKey(byte[] bytes)
        {
            return TryParse(bytes, out PrivateKey key) ? key : null;
        }

        public static implicit operator PrivateKey(string hex)
        {
            return TryParse(hex, out PrivateKey key) ? key : null;
        }

        public static implicit operator byte[] (PrivateKey key)
        {
            return key?.Value;
        }

        public static implicit operator string(PrivateKey key)
        {
            return key?.ToString();
        }
    }
}
