using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Bryllite.Extensions;

namespace Bryllite.Cryptography.Signers
{
    /// <summary>
    /// ECDH signature with R, S, V
    /// </summary>
    public class Signature : Hex
    {
        // signature length
        public const int LENGTH = 1 + Secp256k1Helper.SIGNATURE_LENGTH;

        // R
        public byte[] R
        {
            get { return Value.Take(32).Reverse().ToArray(); }
        }

        // S
        public byte[] S
        {
            get { return Value.Skip(32).Take(32).Reverse().ToArray(); }
        }

        // V
        public byte V => Value.Last();


        public Signature(Hex hex) : base(hex)
        {
            Guard.Assert(Length == LENGTH, "wrong bytes length");
        }

        public Signature(byte[] r, byte[] s, byte v) : this(r.Reverse().Concat(s.Reverse()).Concat(new byte[] { v }).ToArray())
        {
        }

        public PublicKey GetPublicKey(byte[] messageHash)
        {
            Guard.Assert(messageHash.Length == 32, "messageHash.length should be 32");
            return PublicKey.TryParse(Secp256k1Helper.Recover(Value, messageHash), out PublicKey key) ? key : null;
        }


        public static new Signature Parse(byte[] bytes)
        {
            return new Signature(bytes);
        }

        public static new Signature Parse(string hex)
        {
            return new Signature(hex);
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
                signature = null;
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


        public static implicit operator byte[] (Signature signature)
        {
            return signature?.Value;
        }

        public static implicit operator string(Signature signature)
        {
            return signature?.ToString();
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
