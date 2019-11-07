using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Utils.Rlp
{
    public class RlpItem : IRlpItem
    {
        public static readonly RlpItem Null = new RlpItem(null);

        protected byte[] _value;

        public byte[] Value => _value;

        public RlpItem(byte[] value)
        {
            _value = value;
        }
    }
}
