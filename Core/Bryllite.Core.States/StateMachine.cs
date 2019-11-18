using Bryllite.Core.Accounts;
using Bryllite.Cryptography.Signers;
using Bryllite.Database.Trie;
using Bryllite.Database.TrieDB;
using Bryllite.Utils.Currency;
using Bryllite.Utils.NabiLog;
using System;
using System.Collections.Generic;

namespace Bryllite.Core.States
{
    public class StateMachine : Dictionary<Address, Account>, IDisposable, ICloneable
    {
        // statedb
        protected readonly ITrieDB statedb;

        // state root hash
        protected H256 root;
        public H256 RootHash => root;

        // is readonly?
        public bool ReadOnly;

        public StateMachine() : this(new MemoryDB(), null)
        {
        }

        public StateMachine(ITrieDB statedb, H256 root)
        {
            this.statedb = statedb;
            this.root = root;
        }

        public StateMachine(StateMachine other) : base(other)
        {
            statedb = other.statedb;
            root = other.root;
        }

        public void Dispose()
        {
            lock (this)
            {
                // clear dictionary
                Clear();
            }
        }

        public void Reset(H256 root)
        {
            lock (this)
            {
                this.root = root;
                Clear();
            }
        }

        public StateMachine Clone()
        {
            return new StateMachine(this);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Account GetAccount(Address address)
        {
            lock (this)
            {
                if (!ContainsKey(address))
                    using (var trie = new Trie(statedb, root))
                        base[address] = Account.TryParse(trie.Get(address), out var account) ? account : new Account();

                return base[address];
            }
        }

        // memory cached accounts to trie
        public H256 Flush(bool commit)
        {
            lock (this)
            {
                using (var trie = new Trie(statedb, root))
                {
                    foreach (var entry in this)
                    {
                        var address = entry.Key;
                        var account = entry.Value;
                        if (!account.Dirty)
                            continue;

                        // put trie
                        trie.Put(address, account.Rlp);

                        // reset dirty flag if commit
                        if (commit)
                            account.Dirty = false;
                    }

                    return !ReadOnly && commit ? root = trie.Commit() : trie.RootHash;
                }
            }
        }

        public H256 Commit()
        {
            lock (this)
                return Flush(true);
        }

        public void Rollback()
        {
            lock (this)
                Clear();
        }

        // run block
        public H256 Run(Block block)
        {
            if (block.Header.Chain != Tx.MainNet)
                return null;

            try
            {
                lock (this)
                {
                    ulong gasUsed = 0;
                    foreach (var tx in block)
                    {
                        // 각각의 트랜잭션 실행
                        if (!Run(tx))
                            return null;

                        // 소비된 수수료
                        gasUsed += tx.Gas;
                    }

                    // 블록 리워드 지급
                    GetAccount(block.Coinbase).Balance += Coin.GetBlockReward(block.Number);

                    // 수수료 지급
                    GetAccount(block.Coinbase).Balance += gasUsed;

                    return Flush(false);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message);
                return null;
            }
        }

        // run transaction
        public bool Run(Tx tx)
        {
            // can executable?
            if (!CanExecutable(tx))
                return false;

            try
            {
                // sender & receiver
                var sender = GetAccount(tx.From);
                var receiver = GetAccount(tx.To);

                // decrease sender balance & increase sender nonce
                sender.Balance -= tx.Cost;
                sender.Nonce++;

                // increase receiver balance
                receiver.Balance += tx.Value;

                // run data
                RunData(tx.Data);

                // run extra
                RunExtra(tx.Extra);

                return true;
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message);
                return false;
            }
        }

        public bool CanExecutable(Tx tx)
        {
            // network id check
            if (tx.IsCyprusChain)
                return false;

            // check sender nonce & balance
            var sender = GetAccount(tx.From);
            if (sender.Nonce != tx.Nonce || sender.Balance < tx.Cost)
                return false;

            return true;
        }

        private void RunData(byte[] data)
        {
            //throw new NotImplementedException("should implement RunData()");
        }

        private void RunExtra(byte[] extra)
        {
            //throw new NotImplementedException("should implement RunExtra()");
        }
    }
}
