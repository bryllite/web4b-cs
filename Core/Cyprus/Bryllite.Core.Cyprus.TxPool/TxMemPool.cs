using Bryllite.Core.Accounts;
using Bryllite.Core.Cyprus.States;
using Bryllite.Cryptography.Signers;
using Bryllite.Database.TrieDB;
using Bryllite.Utils.NabiLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bryllite.Core.Cyprus.TxPool
{
    public class TxMemPool : IDisposable
    {
        public enum ErrorCode
        {
            None = 0,
            WrongChain,
            KnownTx,
            NonceTooLow,
            InsufficientFunds,
            ReplacementTransactionUnderPriced,
        }

        // statedb
        private ITrieDB statedb;

        // state root 
        private H256 root;
        public H256 RootHash
        {
            get { return root; }
            set { OnStateRootChanged(value); }
        }

        // enqued txs
        private Dictionary<Address, TxList> enqued = new Dictionary<Address, TxList>();

        // pending txs
        private List<Tx> pending = new List<Tx>();

        // pending state
        private StateMachine state;

        // is running?
        public bool Running;



        public TxMemPool()
        {
        }

        public void Dispose()
        {
            lock (this)
            {
                state.Dispose();
                enqued.Clear();
                pending.Clear();
            }
        }

        public void Start(ITrieDB statedb, H256 root)
        {
            if (Running) return;

            this.statedb = statedb;
            this.root = root;

            // state machine
            state = new StateMachine(statedb, root);
            state.ReadOnly = true;
        }

        public void Stop()
        {
            if (!Running) return;

            Dispose();
        }

        public Account GetAccount(Address address)
        {
            return state.GetAccount(address);
        }

        // get account's tx list
        private TxList GetTxList(Address address)
        {
            lock (this)
            {
                if (!enqued.ContainsKey(address))
                    enqued[address] = new TxList();
            }

            return enqued[address];
        }

        public bool Add(Tx tx, out ErrorCode error)
        {
            // sender
            var sender = tx.From;

            lock (this)
            {
                error = ErrorCode.None;
                try
                {
                    // valid chain?
                    if (!tx.IsCyprusChain)
                        return (error = ErrorCode.WrongChain) == ErrorCode.None;

                    var txs = GetTxList(sender);
                    // existing tx?
                    var txid = tx.Txid;
                    if (!ReferenceEquals(txs.Get(txid), null))
                        return (error = ErrorCode.KnownTx) == ErrorCode.None;

                    // sender account
                    var account = GetAccount(tx.From);

                    // valid nonce?
                    if (account.Nonce > tx.Nonce)
                        return (error = ErrorCode.NonceTooLow) == ErrorCode.None;

                    // has enough balance?
                    if (tx.Chain == Tx.Transfer || tx.Chain == Tx.Payout)
                    {
                        if (account.Balance < tx.Cost)
                            return (error = ErrorCode.InsufficientFunds) == ErrorCode.None;
                    }

                    // has duplicated nonce tx?
                    var duplicated = txs.Get(tx.Nonce);
                    if (!ReferenceEquals(duplicated, null))
                    {
                        if (tx.Gas <= duplicated.Gas)
                            return (error = ErrorCode.ReplacementTransactionUnderPriced) == ErrorCode.None;

                        // replace tx
                        Log.Debug("replacing tx: ", duplicated, "=>", tx);
                        txs.Remove(duplicated.Txid);
                    }

                    // add tx
                    txs.Put(tx);
                    return true;
                }
                finally
                {
                    // build executable txs
                    BuildExecutables(root);
                }
            }
        }

        // get tx
        public Tx Get(H256 txid)
        {
            lock (this)
            {
                foreach (var txs in enqued.Values)
                {
                    var tx = txs.Get(txid);
                    if (!ReferenceEquals(tx, null)) return tx;
                }
            }

            return null;
        }

        // discard tx
        public bool Discard(H256 txid)
        {
            lock (this)
            {
                foreach (var txs in enqued.Values)
                {
                    var tx = txs.Get(txid);
                    if (!ReferenceEquals(tx, null)) return txs.Remove(txid);
                }
            }

            return false;
        }

        // get excutable txs
        public IEnumerable<Tx> GetExcutables()
        {
            lock (this)
                return pending.ToArray();
        }

        // get all txs
        public IEnumerable<Tx> GetAllTxs()
        {
            List<Tx> all = new List<Tx>();

            lock (this)
            {
                foreach (var txs in enqued.Values)
                    all.AddRange(txs);
            }

            return all;
        }

        // build pending transactions & state
        private void BuildExecutables(H256 root)
        {
            lock (this)
            {
                // clear previous pending state
                state.Reset(root);
                pending.Clear();

                // random address order
                foreach (var entry in enqued.OrderBy(address => SecureRandom.Next<int>()))
                {
                    var address = entry.Key;
                    var txs = entry.Value;

                    // txs is sorted by nonce
                    foreach (var tx in txs)
                    {
                        if (!state.Run(tx))
                            break;

                        // add pending tx
                        pending.Add(tx);
                    }
                }
            }
        }

        // remove unexcutable transactions
        private void RemoveUnexcutables(H256 root)
        {
            lock (this)
            {
                using (var sm = new StateMachine(statedb, root))
                {
                    foreach (var entry in enqued)
                    {
                        var address = entry.Key;
                        var txs = entry.Value;

                        // txs
                        foreach (var tx in txs)
                        {
                            if (state.Run(tx))
                                break;

                            // remove
                            txs.Remove(tx.Txid);
                        }
                    }
                }
            }
        }

        private void OnStateRootChanged(H256 root)
        {
            lock (this)
            {
                this.root = root;

                // remove unexcutable transactions
                RemoveUnexcutables(root);

                // build pending transactions
                BuildExecutables(root);
            }
        }
    }
}
