using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Bryllite
{
    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] left, byte[] right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.SequenceEqual(right);
        }

        public int GetHashCode(byte[] bytes)
        {
            return new BigInteger(bytes).GetHashCode();
        }
    }
}
