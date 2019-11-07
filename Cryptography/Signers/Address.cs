using System;
using System.Collections.Generic;
using System.Linq;
using Bryllite.Cryptography.Hash;
using Bryllite.Extensions;
using Org.BouncyCastle.Math;

namespace Bryllite.Cryptography.Signers
{
    public class Address : IEquatable<Address>
    {
        // address type
        public enum AddressType : byte
        {
            NA = 0x00,
            EOA = 0xbc,
            IOA = 0x1a,
            CA = 0xca,
            ETH = 0xea,
        }

        // address bytes
        public byte[] Bytes { get; private set; }

        // address hex string
        public string Hex => Bytes.ToHexString();

        // address byte length
        public int Length => Bytes.Length;

        public Address(byte[] bytes)
        {
            Bytes = bytes.ToArray();
        }

        public Address(string hex) : this(hex.ToByteArray())
        {
        }

        public Address(Address other) : this(other.Bytes)
        {
        }

        public static Address Parse(byte[] bytes)
        {
            return new Address(bytes);
        }

        public static Address Parse(string hex)
        {
            return new Address(hex);
        }

        public static bool TryParse(byte[] bytes, out Address address)
        {
            try
            {
                address = Parse(bytes);
                return true;
            }
            catch
            {
                address = null;
                return false;
            }
        }

        public static bool TryParse(string hex, out Address address)
        {
            try
            {
                address = Parse(hex);
                return true;
            }
            catch
            {
                address = null;
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
            return Equals(obj as Address);
        }

        public bool Equals(Address other)
        {
            return !ReferenceEquals(other, null) && Bytes.SequenceEqual(other.Bytes);
        }

        public static bool operator ==(Address left, Address right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Address left, Address right)
        {
            return !(left == right);
        }

        public static implicit operator byte[] (Address address)
        {
            return address?.Bytes;
        }

        public static implicit operator string(Address address)
        {
            return address?.Hex;
        }

        public static implicit operator Address(byte[] bytes)
        {
            return TryParse(bytes, out Address address) ? address : null;
        }

        public static implicit operator Address(string hex)
        {
            return TryParse(hex, out Address address) ? address : null;
        }

        public static implicit operator Hex(Address address)
        {
            return address?.Bytes;
        }

        public static implicit operator Address(Hex hex)
        {
            return TryParse((byte[])hex, out Address address) ? address : null;
        }

        // to EOA Address
        public static Address ToEOA(PublicKey key)
        {
            List<byte> bytes = new List<byte>();

            bytes.Add((byte)AddressType.EOA);
            bytes.AddRange(key.Bytes.Hash256().Skip(12));
            bytes.AddRange(bytes.ToArray().Hash256().Take(3));

            return new Address(bytes.ToArray());
        }

        // to IOA Address
        public static Address ToIOA(PublicKey key)
        {
            List<byte> bytes = new List<byte>();

            bytes.Add((byte)AddressType.IOA);
            bytes.AddRange(key.Bytes.Hash256().Take(20));
            bytes.AddRange(bytes.ToArray().Hash256().Take(3));

            return new Address(bytes.ToArray());
        }

        // to ETH Address
        public static Address ToETH(PublicKey key)
        {
            byte[] hash = key.Key.Hash256();
            return new Address(hash.Skip(12).ToArray());
        }
    }
}
