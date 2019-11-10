using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Utils.Rlp
{
    public class RlpList : List<IRlpItem>, IRlpItem
    {
        public static readonly RlpList Null = new RlpList(null);

        protected byte[] _value;

        public byte[] Value => _value;

        public RlpList(byte[] value)
        {
            _value = value;
        }
    }
}
