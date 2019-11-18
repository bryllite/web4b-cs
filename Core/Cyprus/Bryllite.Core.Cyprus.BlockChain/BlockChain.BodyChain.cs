using Bryllite.Extensions;
using Bryllite.Utils.Rlp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Core.Cyprus
{
    public partial class BlockChain
    {
        public static readonly byte[] BLOCK_PREFIX = Encoding.UTF8.GetBytes("b");
        public static readonly byte[] TX_PREFIX = Encoding.UTF8.GetBytes("t");

        // block hash to block key
        private byte[] ToBlockKey(H256 hash)
        {
            return BLOCK_PREFIX.Append(hash?.Value);
        }

        // txid to tx key
        private byte[] ToTxKey(H256 txid)
        {
            return TX_PREFIX.Append(txid?.Value);
        }

        // get block by hash
        public Block GetBlock(H256 hash)
        {
            return Block.TryParse(chaindb.Get(ToBlockKey(hash)), out Block block) ? block : null;
        }

        public bool TryGetBlock(H256 hash, out Block block)
        {
            try
            {
                block = GetBlock(hash);
                return !ReferenceEquals(block, null);
            }
            catch
            {
                block = null;
                return false;
            }
        }

        // get block by number
        public Block GetBlock(long number)
        {
            return Block.TryParse(chaindb.Get(ToBlockKey(ToBlockHash(number))), out Block block) ? block : null;
        }

        // tx lookup
        public (H256 Hash, long Number, int Index) LookupTx(H256 txid)
        {
            // tx lookup entry
            var rlp = chaindb.Get(ToTxKey(txid));
            if (rlp.IsNullOrEmpty())
                return (null, -1, -1);

            var decoder = new RlpDecoder(rlp);
            H256 hash = decoder.Next();
            long number = Hex.ToNumber<long>(decoder.Next());
            int index = Hex.ToNumber<int>(decoder.Next());

            return (hash, number, index);
        }

        // get tx by block hash, index
        public Tx GetTx(H256 hash, int index)
        {
            return TryGetBlock(hash, out Block block) ? block[index] : null;
        }

        // get tx by block number, index
        public Tx GetTx(long number, int index)
        {
            return GetTx(ToBlockHash(number), index);
        }

        // get tx by txid
        public Tx GetTx(H256 txid)
        {
            var lookup = LookupTx(txid);
            if (ReferenceEquals(lookup.Hash, null))
                return null;

            return GetTx(lookup.Hash, lookup.Index);
        }

        public bool TryGetTx(H256 hash, int index, out Tx tx)
        {
            try
            {
                tx = GetTx(hash, index);
                return !ReferenceEquals(tx, null);
            }
            catch
            {
                tx = null;
                return false;
            }
        }

        public bool TryGetTx(long number, int index, out Tx tx)
        {
            try
            {
                tx = GetTx(number, index);
                return !ReferenceEquals(tx, null);
            }
            catch
            {
                tx = null;
                return false;
            }
        }

        public bool TryGetTx(H256 txid, out Tx tx)
        {
            try
            {
                tx = GetTx(txid);
                return !ReferenceEquals(tx, null);
            }
            catch
            {
                tx = null;
                return false;
            }
        }


        // write block
        private void Write(Block block)
        {
            var hash = block.Hash;
            var header = block.Header;
            var number = block.Number;

            // write header
            Write(header);

            // write body rlp
            chaindb.Put(ToBlockKey(hash), block.Rlp);

            // tx lookup entry
            int index = 0;
            foreach (var tx in block)
                chaindb.Put(ToTxKey(tx.Txid), RlpEncoder.EncodeList(hash, number, index++));
        }
    }
}
