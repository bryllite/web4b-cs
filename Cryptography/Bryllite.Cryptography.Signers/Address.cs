using System;
using System.Collections.Generic;
using System.Linq;
using Bryllite.Cryptography.Hash.Extensions;
using Bryllite.Extensions;
using Org.BouncyCastle.Math;

namespace Bryllite.Cryptography.Signers
{
    public class Address : Hex
    {
        // minimum length of address
        public const int MIN_LENGTH = 20;

        public Address(Hex hex) : base(hex)
        {
            Guard.Assert(Length >= MIN_LENGTH, "address length");
        }

        public static new Address Parse(string hex)
        {
            return new Address(hex);
        }

        public static new Address Parse(byte[] bytes)
        {
            return new Address(bytes);
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

        public static implicit operator Address(string address)
        {
            return TryParse(address, out var hex) ? new Address(hex) : null;
        }

        public static implicit operator Address(byte[] bytes)
        {
            return TryParse(bytes, out var hex) ? new Address(hex) : null;
        }

        public static implicit operator byte[](Address address)
        {
            return address?.Value;
        }

        public static implicit operator string(Address address)
        {
            return address?.ToString();
        }


        public static Address ToETHAddress(PublicKey key)
        {
            return key.Key.Hash256().Skip(12).ToArray();
        }

        public static Address ToAddress(PublicKey key)
        {
            return key.CompressedKey.Hash256().Skip(12).ToArray();
        }

    }
}
