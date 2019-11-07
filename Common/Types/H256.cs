using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite
{
    public class H256 : Hex
    {
        public static readonly int BYTE_LENGTH = 32;

        public H256(byte[] bytes) : base(bytes)
        {
            // check byte length
            Guard.Assert(Length == BYTE_LENGTH, "wrong byte length!");
        }

        public H256(string hex) : base(hex)
        {
        }

        public new static H256 Parse(byte[] bytes)
        {
            return new H256(bytes);
        }

        public new static H256 Parse(string hex)
        {
            return new H256(hex);
        }

        public static bool TryParse(byte[] bytes, out H256 hash)
        {
            try
            {
                hash = Parse(bytes);
                return true;
            }
            catch
            {
                hash = null;
                return false;
            }
        }

        public static bool TryParse(string hex, out H256 hash)
        {
            try
            {
                hash = Parse(hex);
                return true;
            }
            catch
            {
                hash = null;
                return false;
            }
        }

        public static implicit operator H256(byte[] bytes)
        {
            return TryParse(bytes, out var hex) ? hex : null;
        }

        public static implicit operator H256(string str)
        {
            return TryParse(str, out var hex) ? hex : null;
        }

        public static implicit operator byte[] (H256 hash)
        {
            return hash?.value;
        }

        public static implicit operator string(H256 hash)
        {
            return hash?.ToString();
        }
    }
}
