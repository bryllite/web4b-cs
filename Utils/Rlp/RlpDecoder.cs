using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Utils.Rlp
{
    public class RlpDecoder : RlpBase, IEnumerable
    {
        // rlp 원본 데이터
        private byte[] rlp;

        // rlp 아이템 ( RLPItem, RLPCollection )
        private IRlpItem item;

        // RLP 아이템 갯수
        public int Count => item is RlpList list ? list.Count : 1;

        public IEnumerator GetEnumerator()
        {
            if (item is RlpList list)
                return list.GetEnumerator();

            throw new Exception("GetEnumerator() supported only RLPList");
        }

        // RLP 아이템 밸류
        public byte[] Value => item.Value;

        // 인덱서
        // 인덱싱은 RLPList 타입에서만 지원된다
        public IRlpItem this[int idx]
        {
            get
            {
                if (item is RlpList list)
                    return list[idx];

                throw new Exception("RLP is not list type");
            }
        }

        public RlpDecoder(byte[] rlp)
        {
            this.rlp = rlp;

            // RLP decode
            item = Decode(rlp);
        }

        public RlpDecoder Clone()
        {
            return new RlpDecoder(rlp);
        }

        public static implicit operator byte[] (RlpDecoder rlp)
        {
            return rlp.rlp;
        }

        private int pos = 0;
        public byte[] Next()
        {
            try
            {
                return this[pos++].Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
