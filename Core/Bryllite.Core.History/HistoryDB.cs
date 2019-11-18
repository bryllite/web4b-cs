using Bryllite.Cryptography.Signers;
using Bryllite.Database.Trie;
using Bryllite.Database.TrieDB;
using Bryllite.Extensions;
using Bryllite.Utils.NabiLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Core.History
{
    public class HistoryDB : IDisposable
    {
        // address key prefix
        public static readonly byte[] ADDRESS_PREFIX = Encoding.UTF8.GetBytes("a");

        // history db
        private ITrieDB db;

        // db running?
        public bool Running => db.Running;

        public HistoryDB(ITrieDB db)
        {
            this.db = db;
        }

        // use memory db
        public HistoryDB() : this(new MemoryDB())
        {
        }

        // use file db
        public HistoryDB(string path) : this(new FileDB(path))
        {
        }

        public void Dispose()
        {
            db.Stop();
        }

        public void Start()
        {
            // start db
            db.Start();
        }

        public void Stop()
        {
            Dispose();
        }

        private byte[] ToAddressKey(Address address)
        {
            return ADDRESS_PREFIX.Append(address);
        }

        // get address history root
        private H256 GetHistoryRoot(Address address)
        {
            return db.Get(ToAddressKey(address));
        }

        private void SetHistoryRoot(Address address, H256 root)
        {
            db.Put(ToAddressKey(address), root);
        }

        // add transaction to history
        public bool Put(Tx tx)
        {
            H256 txid = tx.Txid;

            Address sender = tx.From;
            Address receiver = tx.To;

            try
            {
                // add sender tx
                if (!ReferenceEquals(sender, null))
                {
                    H256 root = GetHistoryRoot(sender);
                    using (var trie = new Trie(db, root))
                    {
                        trie.Put(txid, tx.Rlp);
                        root = trie.Commit();

                        // change address history root
                        SetHistoryRoot(sender, root);
                    }
                }

                // add receiver tx
                if (!ReferenceEquals(receiver, null))
                {
                    H256 root = GetHistoryRoot(receiver);
                    using (var trie = new Trie(db, root))
                    {
                        trie.Put(txid, tx.Rlp);
                        root = trie.Commit();

                        // change address history root
                        SetHistoryRoot(receiver, root);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message);
                return false;
            }
        }

        // get tx history of address
        public IEnumerable<Tx> Get(Address address)
        {
            List<Tx> txs = new List<Tx>();

            H256 root = GetHistoryRoot(address);
            using (var trie = new Trie(db, root))
            {
                foreach (var entry in trie)
                {
                    // key: txid
                    H256 txid = entry.Key;

                    // value: tx.rlp
                    if (Tx.TryParse(entry.Value, out Tx tx))
                        txs.Add(tx);
                }
            }

            return txs;
        }
    }
}
