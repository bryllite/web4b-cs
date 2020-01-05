using Bryllite.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bryllite.Database.TrieDB
{
    public class MemoryDB : ITrieDB
    {
        // dictionary for key/value db
        private Dictionary<byte[], byte[]> db;

        // is db running?
        public bool Running
        {
            get
            {
                lock(this)
                    return !ReferenceEquals(db, null);
            }
        }
        
        public IEnumerable<byte[]> Keys
        {
            get
            {
                lock (this)
                    return db.Keys.ToArray();
            }
        }

        public IEnumerable<byte[]> Values
        {
            get
            {
                lock (this)
                    return db.Values.ToArray();
            }
        }

        public MemoryDB()
        {
        }

        public void Dispose()
        {
            lock (this)
            {
                db?.Clear();
                db = null;
            }
        }

        public void Start()
        {
            lock (this)
            {
                Guard.Assert(db == null, "already started");
                db = new Dictionary<byte[], byte[]>(new ByteArrayComparer());
            }
        }

        public void Stop()
        {
            Dispose();
        }


        public byte[] Get(byte[] key)
        {
            try
            {
                lock (this)
                    return Has(key) ? db[key] : null;
            }
            catch
            {
                return null;
            }
        }

        public bool TryGet(byte[] key, out byte[] value)
        {
            try
            {
                lock (this)
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
            try
            {
                lock (this)
                {
                    db[key] = value;
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool Del(byte[] key)
        {
            try
            {
                lock (this)
                    return db.Remove(key);
            }
            catch
            {
                return false;
            }
        }

        public bool Has(byte[] key)
        {
            try
            {
                lock (this)
                    return db.ContainsKey(key);
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<KeyValuePair<byte[], byte[]>> AsEnumerable()
        {
            lock (this)
                return db.AsEnumerable().ToArray();
        }
    }
}
