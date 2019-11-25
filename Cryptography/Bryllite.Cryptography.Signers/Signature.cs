using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Bryllite.Extensions;

namespace Bryllite.Cryptography.Signers
{
    /// <summary>
    /// R, S, V 를 모두 포함하는 65바이트 크기의 서명 정보
    /// </summary>
    public class Signature
    {
        public static readonly int BYTE_LENGTH = 1 + Secp256k1Helper.SIGNATURE_LENGTH;
        public static readonly Signature Null = null;

        public byte[] Bytes { get; private set; }

        public string Hex => Bytes.ToHexString();

        public byte[] R
        {
            get
            {
                return Bytes.Take(32).Reverse().ToArray();
            }
        }

        public byte[] S
        {
            get
            {
                return Bytes.Skip(32).Take(32).Reverse().ToArray();
            }
        }

        public byte V
        {
            get
            {
                return Bytes.Last();
            }
        }

        protected Signature()
        {
        }

        public Signature(byte[] bytes)
        {
            Guard.Assert(bytes.Length == BYTE_LENGTH);
            Bytes = bytes.ToArray();
        }

        public Signature(byte[] r, byte[] s, byte v) : this(r.Append(s).Append(v))
        {
        }

        public Signature(string hex) : this(hex.ToByteArray())
        {
        }

        public Signature(Signature other) : this(other.Bytes)
        {
        }

        public PublicKey GetPublicKey(byte[] messageHash)
        {
            Guard.Assert(!messageHash.IsNullOrEmpty() && messageHash.Length == 32, "messageHash.length should be 32");
            return PublicKey.TryParse(Secp256k1Helper.Recover(Bytes, messageHash), out PublicKey key) ? key : null;
        }


        public byte[] ToByteArray()
        {
            return Bytes;
        }

        public static Signature Parse(byte[] bytes)
        {
            return !bytes.IsNullOrEmpty() ? new Signature(bytes) : Null;
        }

        public static Signature Parse(string hex)
        {
            return Parse(hex.ToByteArray());
        }

        public static bool TryParse(byte[] bytes, out Signature signature)
        {
            try
            {
                signature = Parse(bytes);
                return !ReferenceEquals(signature, null);
            }
            catch (Exception)
            {
                signature = Null;
                return false;
            }
        }

        public static bool TryParse(string hex, out Signature signature)
        {
            try
            {
                signature = Parse(hex);
                return !ReferenceEquals(signature, null);
            }
            catch (Exception)
            {
                signature = null;
                return false;
            }
        }

        public override string ToString()
        {
            return Hex;
        }

        public override int GetHashCode()
        {
            return new BigInteger(Bytes).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as Signature;
            return !ReferenceEquals(o, null) && Bytes.SequenceEqual(o.Bytes);
        }

        public static bool operator ==(Signature left, Signature right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }
        public static bool operator !=(Signature left, Signature right)
        {
            return !(left == right);
        }

        public static implicit operator byte[] (Signature signature)
        {
            return signature?.Bytes;
        }

        public static implicit operator string(Signature signature)
        {
            return signature?.Hex;
        }

        public static implicit operator Signature(byte[] bytes)
        {
            return TryParse(bytes, out var signature) ? signature : null;
        }

        public static implicit operator Signature(string hex)
        {
            return TryParse(hex, out var signature) ? signature : null;
        }

    }
}
