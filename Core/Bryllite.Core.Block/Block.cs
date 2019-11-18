using Bryllite.Cryptography.Signers;
using Bryllite.Database.Trie;
using Bryllite.Extensions;
using Bryllite.Utils.Rlp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bryllite.Core
{
    public class Block : ICloneable, IEnumerable<Tx>
    {
        public static readonly Block Empty = new Block();

        // block header
        public BlockHeader Header { get; private set; }

        // block body ( transaction list )
        public List<Tx> Body { get; private set; }

        // tx indexer
        public Tx this[int index] => Body.Count > index ? Body.ElementAt(index) : null;
        public Tx this[Hex txid] => Body.FirstOrDefault(tx => tx.Txid == txid);

        // block header access
        public H256 Hash => Header.Hash;
        public long Number => Header.Number;
        public Address Coinbase => Header.Coinbase;
        public H256 ParentHash => Header.ParentHash;
        public H256 TransactionRoot => Header.TransactionRoot;
        public H256 StateRoot => Header.StateRoot;


        // total gas used of all txs
        public ulong GasUsed
        {
            get
            {
                ulong used = 0;
                foreach (var tx in Body)
                    used += tx.Gas;
                return used;
            }
        }

        // block size
        public int Size => Rlp.Length;

        // rlp stream
        public byte[] Rlp => ToRlp();

        public Block()
        {
            Header = new BlockHeader();
            Body = new List<Tx>();
        }

        public Block(BlockHeader header)
        {
            Header = header;
            Body = new List<Tx>();
        }

        public Block(BlockHeader header, IEnumerable<Tx> body)
        {
            Header = header;
            Body = new List<Tx>(body);

            Header.TransactionRoot = ToMerkleRoot(Body);
        }

        protected Block(byte[] rlp)
        {
            var decoder = new RlpDecoder(rlp);

            // header
            Header = BlockHeader.Parse(decoder.Next());

            // body
            Body = ToBody(decoder.Next());
        }

        protected Block(string rlp) : this(Hex.ToByteArray(rlp))
        {
        }

        public IEnumerator<Tx> GetEnumerator()
        {
            return Body.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return Hash;
        }

        public JObject ToJObject(bool fulltx)
        {
            // header json
            var json = Header.ToJObject();

            // transactions
            JArray txs = new JArray();
            foreach (var tx in Body)
            {
                if (fulltx) txs.Add(tx.ToJObject());
                else txs.Add(tx.Txid.ToString());
            }
            json.Put("transactions", txs);
            json.Put("size", Hex.ToString(Size, true));
            json.Put("gasUsed", Hex.ToString(GasUsed, true));

            return json;
        }

        public Block Clone()
        {
            return new Block(Rlp);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        protected virtual byte[] ToRlp()
        {
            return RlpEncoder.EncodeList(Header.Rlp, ToRlp(Body));
        }


        public static byte[] ToRlp(IEnumerable<Tx> body)
        {
            return RlpEncoder.EncodeList(body.Select(tx => tx.Rlp).ToArray());
        }

        public static List<Tx> ToBody(byte[] rlp)
        {
            var body = new List<Tx>();
            if (!rlp.IsNullOrEmpty())
            {
                var decoder = new RlpDecoder(rlp);
                for (int i = 0; i < decoder.Count; i++)
                    body.Add(Tx.Parse(decoder[i].Value));
            }

            return body;
        }

        public static H256 ToMerkleRoot(IEnumerable<Tx> txs)
        {
            using (var trie = new Trie())
            {
                foreach (var tx in txs)
                    trie.Put(tx.Txid, tx.Rlp);

                return trie.RootHash;
            }
        }

        public static Block Parse(byte[] rlp)
        {
            return new Block(rlp);
        }

        public static Block Parse(string rlp)
        {
            return new Block(rlp);
        }

        public static bool TryParse(byte[] rlp, out Block block)
        {
            try
            {
                block = Parse(rlp);
                return true;
            }
            catch
            {
                block = null;
                return false;
            }
        }

        public static bool TryParse(string rlp, out Block block)
        {
            try
            {
                block = Parse(rlp);
                return true;
            }
            catch
            {
                block = null;
                return false;
            }
        }
    }
}
