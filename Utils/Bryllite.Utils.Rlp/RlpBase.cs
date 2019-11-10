using Bryllite.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Utils.Rlp
{
    public class RlpBase
    {
        public const byte SHORT_ITEM_LENGTH = 55;

        public const byte ITEM_OFFSET = 0x80;
        public const byte LONG_ITEM_OFFSET = ITEM_OFFSET + SHORT_ITEM_LENGTH;   // 0xb7
        public const byte LIST_OFFSET = 0xc0;
        public const byte LONG_LIST_OFFSET = LIST_OFFSET + SHORT_ITEM_LENGTH;   // 0xf7

        public static readonly byte[] EmptyItem = new byte[] { ITEM_OFFSET };
        public static readonly byte[] EmptyList = new byte[] { LIST_OFFSET };


        protected static bool TryDecode(byte[] rlp, out IRlpItem items)
        {
            try
            {
                items = Decode(rlp, 0);
                return true;
            }
            catch
            {
                items = null;
                return false;
            }
        }

        protected static IRlpItem Decode(byte[] rlp)
        {
            return Decode(rlp, 0);
        }

        protected static IRlpItem Decode(byte[] rlp, int offset)
        {
            return Decode(rlp, ref offset);
        }

        // RLP 디코딩
        protected static IRlpItem Decode(byte[] rlp, ref int cursor)
        {
            if (ReferenceEquals(rlp, null) || rlp.Length == 0 || rlp.Length <= cursor) throw new ArgumentNullException(nameof(rlp));

            byte prefix = rlp[cursor++];

            // null item, null list, single byte
            if (prefix == ITEM_OFFSET) return RlpItem.Null;
            if (prefix == LIST_OFFSET) return RlpList.Null;
            if (prefix < ITEM_OFFSET) return new RlpItem(prefix.ToByteArray());

            // short item
            if (prefix <= LONG_ITEM_OFFSET)
            {
                return new RlpItem(rlp.Slice(ref cursor, prefix - ITEM_OFFSET));
            }
            // long item
            else if (prefix < LIST_OFFSET)
            {
                var byteNum = prefix - LONG_ITEM_OFFSET;
                int length = FromBinary(rlp.Slice(ref cursor, byteNum));

                return new RlpItem(rlp.Slice(ref cursor, length));
            }
            // short list
            else if (prefix <= LONG_LIST_OFFSET)
            {
                var length = prefix - LIST_OFFSET;
                RlpList collection = new RlpList(rlp.Slice(cursor - 1, length + 1));

                int end = cursor + length;
                while (cursor < end)
                    collection.Add(Decode(rlp, ref cursor));

                return collection;
            }
            // long list
            else
            {
                var byteNum = prefix - LONG_LIST_OFFSET;
                int length = FromBinary(rlp.Slice(ref cursor, byteNum));
                RlpList collection = new RlpList(rlp.Slice(cursor - byteNum - 1, length + byteNum + 1));

                int end = cursor + length;
                while (cursor < end)
                    collection.Add(Decode(rlp, ref cursor));

                return collection;
            }
        }

        public static byte[] EncodeItem(byte[] item)
        {
            if (item.IsNullOrEmpty()) return EmptyItem;
            if (item.Length == 1 && item[0] < ITEM_OFFSET) return item;

            return EncodePrefix(item.Length, ITEM_OFFSET).Append(item);
        }

        public static byte[] EncodeList(IEnumerable<byte[]> items)
        {
            byte[] data = new byte[0];
            if (!ReferenceEquals(items, null))
            {
                foreach (var item in items)
                    data = data.Append(EncodeItem(item));
            }

            return EncodePrefix(data.Length, LIST_OFFSET).Append(data);
        }

        // object 인코딩한다.
        public static byte[] EncodeObject(object item)
        {
            if (ReferenceEquals(item, null)) return EmptyItem;

            // primitive types ( encode bigendian )
            if (item is bool b) return EncodeItem(b.ToByteArray(true));
            else if (item is byte by) return EncodeItem(by.ToByteArray(true));
            else if (item is sbyte sb) return EncodeItem(sb.ToByteArray(true));
            else if (item is short s) return EncodeItem(s.ToByteArray(true));
            else if (item is ushort us) return EncodeItem(us.ToByteArray(true));
            else if (item is int i) return EncodeItem(i.ToByteArray(true));
            else if (item is uint ui) return EncodeItem(ui.ToByteArray(true));
            else if (item is long l) return EncodeItem(l.ToByteArray(true));
            else if (item is ulong ul) return EncodeItem(ul.ToByteArray(true));
            else if (item is float f) return EncodeItem(f.ToByteArray(true));
            else if (item is double d) return EncodeItem(d.ToByteArray(true));
            else if (item is decimal dec) return EncodeItem(dec.ToByteArray(true));
            // byte[], string, hex
            else if (item is byte[] bytes) return EncodeItem(bytes);
            else if (item is string str) return EncodeItem(System.Text.Encoding.UTF8.GetBytes(str));
            else if (item is Hex hex) return EncodeItem(hex);
            // list
            else if (item is IEnumerable enumerable)
            {
                byte[] data = new byte[0];
                foreach (object o in enumerable)
                    data = data.Append(EncodeObject(o));

                return EncodePrefix(data.Length, LIST_OFFSET).Append(data);
            }

            throw new FormatException("unsupported type");
        }

        public static byte[] EncodePrefix(int length, byte offset)
        {
            // offset should be 0x80 or 0xc0
            Guard.Assert(offset == ITEM_OFFSET || offset == LIST_OFFSET);

            if (length <= SHORT_ITEM_LENGTH)
                return new byte[] { (byte)(offset + length) };

            byte[] bytes = ToBinary(length);
            byte prefix = (byte)(offset + SHORT_ITEM_LENGTH + bytes.Length);
            return new byte[] { prefix }.Append(bytes);
        }

        // 길이를 저장하기 위해 필요한 바이트 수를 구한다.
        // 원래는 8바이트 까지 저장할 수 있지만, 여기서는 int.MaxValue까지만 사용하도록 한다. 
        // 인덱스 등이 주로 int기 때문에 long 사용하기가 불편하다.
        // 1: 1 - 255
        // 2: 256 - 65,536
        // 3: 65,537 - 16,777,216
        // 4: 16,777,217 - 2,147,483,647 (int.MaxValue)
        protected static byte EstimatedBytes(int length)
        {
            byte n = 0;
            while (length > 0)
            {
                ++n;
                length >>= 8;
            }

            return n;
        }

        // 바이너리를 길이로 변환한다( BigEndian )
        protected static int FromBinary(byte[] bytes)
        {
            Guard.Assert(bytes.Length <= sizeof(int));

            int length = 0;
            byte pow = (byte)(bytes.Length - 1);

            for (int i = 0; i < bytes.Length; i++)
            {
                length += (bytes[i] << 8 * pow);
                pow--;
            }

            return length;
        }

        // 길이를 바이너리로 변환한다( BigEndian )
        protected static byte[] ToBinary(int length)
        {
            byte byteNum = EstimatedBytes(length);

            byte[] bytes = new byte[byteNum];
            for (var i = 0; i < byteNum; i++)
                bytes[byteNum - 1 - i] = (byte)(length >> (8 * i));

            return bytes;
        }

    }
}
