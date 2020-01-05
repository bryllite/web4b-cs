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
    public class PublicKey : Hex
    {
        public const int KEY_LENGTH = Secp256k1Helper.PUBLIC_KEY_LENGTH;
        public const int COMPRESSED_KEY_LENGTH = Secp256k1Helper.SERIALIZED_COMPRESSED_PUBKEY_LENGTH;
        public const int UNCOMPRESSED_LEY_LENGTH = Secp256k1Helper.SERIALIZED_UNCOMPRESSED_PUBKEY_LENGTH;

        // compressed key
        public byte[] CompressedKey => PublicKeySerialize(Value, true);

        // uncompressed key
        public byte[] UncompressedKey => PublicKeySerialize(Value, false);

        // public key
        public byte[] Key => UncompressedKey.Skip(1).ToArray();

        // x coordinates
        public H256 X => Key.Left();

        // y coordinates
        public H256 Y => Key.Right();

        public Address Address => Address.ToETHAddress(this);


        public PublicKey(Hex hex) 
        {
            switch (hex.Length)
            {
                case KEY_LENGTH:
                    value = hex;
                    return;
                case COMPRESSED_KEY_LENGTH:
                case UNCOMPRESSED_LEY_LENGTH:
                    value = PublicKeyParse(hex);
                    return;

                default:
                    break;
            }

            throw new FormatException("wrong public key bytes");
        }

        public static byte[] PublicKeyParse(byte[] bytes)
        {
            return Secp256k1Helper.PublicKeyParse(bytes);
        }

        public static byte[] PublicKeySerialize(byte[] key, bool compress)
        {
            return Secp256k1Helper.PublicKeySerialize(key, compress);
        }

        public static new PublicKey Parse(byte[] bytes)
        {
            return new PublicKey(bytes);
        }

        public static new PublicKey Parse(string hex)
        {
            return new PublicKey(hex);
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
                key = null;
                return false;
            }
        }

        public static bool TryParse(string hex, out PublicKey key)
        {
            try
            {
                key = Parse(hex);
                return !ReferenceEquals(key, null);
            }
            catch (Exception)
            {
                key = null;
                return false;
            }
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public new string ToString(bool compress)
        {
            return compress ? ToString(CompressedKey) : ToString(UncompressedKey);
        }

        public bool Verify(byte[] signature, byte[] messageHash)
        {
            Guard.Assert(messageHash.Length == 32, "messageHash.length should be 32");
            return Secp256k1Helper.Verify(signature, messageHash, Value);
        }

        public bool Verify(Signature signature, byte[] messageHash)
        {
            return Verify((byte[])signature, messageHash);
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
            return key?.Value;
        }

        public static implicit operator string(PublicKey key)
        {
            return key?.ToString();
        }
    }
}
