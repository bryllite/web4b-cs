using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bryllite.Core
{
    public class TxList : IEnumerable, IEnumerable<Tx>
    {
        // txs
        private Dictionary<H256, Tx> txs = new Dictionary<H256, Tx>();

        public TxList()
        {
        }

        // nonce ordered list
        public IEnumerator<Tx> GetEnumerator()
        {
            foreach (var tx in ToOrdered())
                yield return tx;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            lock (txs)
                txs.Clear();
        }

        public Tx Get(H256 txid)
        {
            lock (txs)
                return txs.TryGetValue(txid, out Tx tx) ? tx : null;
        }

        public Tx Get(ulong nonce)
        {
            lock (txs)
                return txs.Values.FirstOrDefault(tx => tx.Nonce == nonce);
        }

        public void Put(Tx tx)
        {
            lock (txs)
                txs[tx.Txid] = tx;
        }

        public bool Remove(H256 txid)
        {
            lock (txs)
                return txs.Remove(txid);
        }

        public IEnumerable<Tx> ToOrdered()
        {
            lock (txs)
                return txs.Values.OrderBy(tx => tx.Nonce);
        }
    }
}
