using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite
{
    public class Nibble
    {
        public static readonly Nibble Null = new Nibble() { Value = byte.MaxValue };

        public const int MaxValue = 0x0f;
        public const int MinValue = 0x00;

        public byte Value = 0;

        public bool IsNull => Value < MinValue || Value > MaxValue;

        public Nibble()
        {
        }


        public Nibble(byte nibble)
        {
            Value = (byte)(nibble & 0x0F);
        }

        public override string ToString()
        {
            return Value.ToString("x1");
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Nibble n ? Value == n.Value : false;
        }

        public static bool operator ==(Nibble left, Nibble right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Nibble left, Nibble right)
        {
            return !(left == right);
        }

        public static implicit operator byte(Nibble nibble)
        {
            return nibble.Value;
        }

        public static implicit operator string(Nibble nibble)
        {
            return nibble.ToString();
        }

        public static implicit operator Nibble(byte nibble)
        {
            return new Nibble(nibble);
        }

    }
}
