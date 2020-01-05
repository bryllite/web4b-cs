using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite
{
    public class H160 : Hex
    {
        public static readonly int BYTE_LENGTH = 20;

        // max value
        public static readonly H160 MaxValue = "0xffffffffffffffffffffffffffffffffffffffff";
        // min value
        public static readonly H160 MinValue = "0x0000000000000000000000000000000000000000";

        public H160(byte[] bytes) : base(bytes)
        {
            Guard.Assert(Length == BYTE_LENGTH, "wrong bytes length!");
        }

        public H160(string hex) : base(hex)
        {
        }

        public new static H160 Parse(byte[] bytes)
        {
            return new H160(bytes);
        }

        public new static H160 Parse(string hex)
        {
            return new H160(hex);
        }

        public static bool TryParse(byte[] bytes, out H160 h160)
        {
            try
            {
                h160 = Parse(bytes);
                return true;
            }
            catch
            {
                h160 = null;
                return false;
            }
        }

        public static bool TryParse(string hex, out H160 h160)
        {
            try
            {
                h160 = Parse(hex);
                return true;
            }
            catch
            {
                h160 = null;
                return false;
            }
        }

        public static implicit operator H160(byte[] bytes)
        {
            return TryParse(bytes, out var hex) ? hex : null;
        }

        public static implicit operator H160(string str)
        {
            return TryParse(str, out var hex) ? hex : null;
        }

        public static implicit operator byte[] (H160 h160)
        {
            return h160?.value;
        }

        public static implicit operator string(H160 h160)
        {
            return h160?.ToString();
        }
    }
}
