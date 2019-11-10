using Bryllite.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Database.TrieDB
{
    public class MemoryDB : ITrieDB
    {
        // dictionary for key/value db
        private Dictionary<byte[], byte[]> db;

        // is db running?
        public bool Running => !ReferenceEquals(db, null);

        public IEnumerable<byte[]> Keys => db.Keys;
        public IEnumerable<byte[]> Values => db.Values;

        public MemoryDB()
        {
            Start();
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            return db.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void Dispose()
        {
            if (db != null)
            {
                lock (db)
                {
                    db.Clear();
                    db = null;
                }
            }
        }

        public void Start()
        {
            Guard.Assert(db == null, "already started");
            db = new Dictionary<byte[], byte[]>(new ByteArrayComparer());
        }

        public void Stop()
        {
            Dispose();
        }


        public byte[] Get(byte[] key)
        {
            if (db == null || key.IsNullOrEmpty()) return null;

            lock (db)
                return Has(key) ? db[key] : null;
        }

        public bool TryGet(byte[] key, out byte[] value)
        {
            try
            {
                lock (db)
                {
                    value = db[key];
                    return true;
                }
            }
            catch
            {
                value = null;
                return false;
            }
        }

        public bool Put(byte[] key, byte[] value)
        {
            if (db == null || key.IsNullOrEmpty()) return false;

            lock (db)
                db[key] = value;

            return true;
        }

        public bool Del(byte[] key)
        {
            if (db == null || key.IsNullOrEmpty()) return false;

            lock (db)
                return db.Remove(key);
        }

        public bool Has(byte[] key)
        {
            if (db == null || key.IsNullOrEmpty()) return false;

            lock (db)
                return db.ContainsKey(key);
        }
    }
}
