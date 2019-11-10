using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Utils.Rlp
{
    /// <summary>
    /// RLP 인코더
    /// </summary>
    public class RlpEncoder : RlpBase
    {
        private static readonly byte[] Null = new byte[0];

        // 아이템 목록
        private List<object> items;

        // 리스트로 인코딩할까?
        private bool isList;

        public RlpEncoder() : this(false)
        {
        }

        public RlpEncoder(bool list)
        {
            items = new List<object>();
            isList = list;
        }

        public RlpEncoder(params byte[][] items) : this()
        {
            isList = items.Length > 1;
            if (ReferenceEquals(items, null)) return;

            foreach (var item in items)
                Add(item);
        }

        public void Add(object item)
        {
            items.Add(item);
        }

        public byte[] Encode()
        {
            if (items.Count == 0) return isList ? EmptyList : EmptyItem;
            if (items.Count == 1 && !isList) return EncodeObject(items[0]);
            return EncodeObject(items);
        }

        public static RlpEncoder New(int capacity, byte[] value = null)
        {
            Guard.Assert(capacity > 0);

            var encoder = new RlpEncoder(true);
            for (int i = 0; i < capacity; i++)
                encoder.Add(ReferenceEquals(value, null) ? Null : value);

            return encoder;
        }

        public static byte[] EncodeList(params object[] items)
        {
            var encoder = new RlpEncoder(true);
            foreach (var item in items)
                encoder.Add(item);

            return encoder.Encode();
        }

        public static byte[] Encode(object item)
        {
            var encoder = new RlpEncoder(false);
            encoder.Add(item);
            return encoder.Encode();
        }
    }
}
