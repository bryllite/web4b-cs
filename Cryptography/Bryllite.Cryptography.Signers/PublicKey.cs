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
    public class PublicKey
    {
        public const int KEY_LENGTH = Secp256k1Helper.PUBLIC_KEY_LENGTH;
        public const int KEY_COMPRESSED_LENGTH = Secp256k1Helper.SERIALIZED_COMPRESSED_PUBKEY_LENGTH;
        public const int KEY_UNCOMPRESSED_LENGTH = Secp256k1Helper.SERIALIZED_UNCOMPRESSED_PUBKEY_LENGTH;

        public static readonly PublicKey Null = new PublicKey();

        public byte[] Bytes { get; private set; }

        // compressed key
        public byte[] CompressedKey => PublicKeySerialize(Bytes, true);

        // uncompressed key
        public byte[] UncompressedKey => PublicKeySerialize(Bytes, false);

        // public key readable
        public byte[] Key => UncompressedKey.Skip(1).ToArray();

        // x, y
        public byte[] X => Key.Left();
        public byte[] Y => Key.Right();

        public H256 Hash => Key.Hash256();

        // To Address
        public Address Address => Address.ToETH(this);


        protected PublicKey()
        {
        }

        public PublicKey(byte[] bytes) : this()
        {
            switch (bytes.Length)
            {
                case KEY_LENGTH:
                    Bytes = bytes.ToArray();
                    return;

                case KEY_COMPRESSED_LENGTH:
                case KEY_UNCOMPRESSED_LENGTH:
                    Bytes = PublicKeyParse(bytes);
                    return;

                default:
                    break;
            }

            throw new FormatException("invalid key bytes!");
        }

        public byte[] ToByteArray()
        {
            return Bytes;
        }

        public static byte[] PublicKeyParse(byte[] bytes)
        {
            return Secp256k1Helper.PublicKeyParse(bytes);
        }

        public static byte[] PublicKeySerialize(byte[] key, bool compress)
        {
            return Secp256k1Helper.PublicKeySerialize(key, compress);
        }

        public static PublicKey Parse(byte[] bytes)
        {
            return new PublicKey(bytes);
        }

        public static PublicKey Parse(string hex)
        {
            return Parse(hex.ToByteArray());
        }

        public static bool TryParse(byte[] bytes, out PublicKey key)
        {
            try
            {
                key = Parse(bytes);
                return !ReferenceEquals(key, null);
            }
            catch (Exception)
            {
                key = Null;
                return false;
            }
        }

        public static bool TryParse(string hex, out PublicKey key)
        {
            try
            {
                key = Parse(hex.ToByteArray());
                return !ReferenceEquals(key, null);
            }
            catch (Exception)
            {
                key = Null;
                return false;
            }
        }

        public override string ToString()
        {
            return Hex.ToString(UncompressedKey);
        }

        public override int GetHashCode()
        {
            return new BigInteger(Bytes).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as PublicKey;
            return !ReferenceEquals(o, null) && Bytes.SequenceEqual(o.Bytes);
        }

        public static bool operator ==(PublicKey left, PublicKey right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(PublicKey left, PublicKey right)
        {
            return !(left == right);
        }

        public bool Verify(byte[] signature, byte[] messageHash)
        {
            Guard.Assert(!messageHash.IsNullOrEmpty() && messageHash.Length == 32, "messageHash.length should be 32");
            return Secp256k1Helper.Verify(signature, messageHash, Bytes);
        }

        public bool Verify(Signature signature, byte[] messageHash)
        {
            return Verify(signature.Bytes, messageHash);
        }

        public static implicit operator PublicKey(byte[] bytes)
        {
            return TryParse(bytes, out PublicKey key) ? key : null;
        }

        public static implicit operator PublicKey(string hex)
        {
            return TryParse(hex, out PublicKey key) ? key : null;
        }

        public static implicit operator byte[] (PublicKey key)
        {
            return key?.ToByteArray();
        }

        public static implicit operator string(PublicKey key)
        {
            return key?.ToString();
        }

    }
}
