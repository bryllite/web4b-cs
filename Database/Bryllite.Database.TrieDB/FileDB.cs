using Bryllite.Extensions;
using LevelDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bryllite.Database.TrieDB
{
    public class FileDB : ITrieDB
    {
        // default option
        public static readonly Options DefaultOptions = new Options() { CreateIfMissing = true, CompressionLevel = CompressionLevel.SnappyCompression };

        // default encoding
        public static readonly Encoding DefaultEncoding = Encoding.UTF8;

        // level db for physical db
        private DB db;

        private string path;
        private Options options;
        private Encoding encoding;

        // db path
        public string DataPath => path;

        public IEnumerable<byte[]> Keys => AsEnumerable().Select(kv => kv.Key);

        public IEnumerable<byte[]> Values => AsEnumerable().Select(kv => kv.Value);


        public bool Running
        {
            get
            {
                lock(this)
                    return !ReferenceEquals(db, null);
            }
        }

        public FileDB(string path) : this(path, DefaultOptions, DefaultEncoding)
        {
        }

        public FileDB(string path, bool start) : this(path, DefaultOptions, DefaultEncoding)
        {
            if (start) Start();
        }

        public FileDB(string path, Options options) : this(path, options, DefaultEncoding)
        {
        }

        public FileDB(string path, Options options, Encoding encoding)
        {
            this.path = path;
            this.options = options;
            this.encoding = encoding;
        }

        public void Dispose()
        {
            lock (this)
            {
                db?.Close();
                db = null;
            }
        }

        public void Start()
        {
            lock (this)
            {
                Guard.Assert(db == null, "already started");

                path.MakeSureDirectoryPathExists();
                db = new DB(options, path, encoding);
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
                    return db.Get(key);
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
                    value = db.Get(key);
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
                    db.Put(key, value);
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
                {
                    db.Delete(key);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool Has(byte[] key)
        {
            lock(this)
                return !ReferenceEquals(Get(key), null);
        }

        public IEnumerable<KeyValuePair<byte[], byte[]>> AsEnumerable()
        {
            List<KeyValuePair<byte[], byte[]>> enumerable = new List<KeyValuePair<byte[], byte[]>>();
            lock (this)
            {
                foreach (var entry in db)
                    enumerable.Add(entry);
            }
            return enumerable;
        }


        public bool Repair()
        {
            lock(this)
                return !Running ? Repair(path) : false;
        }

        public bool Destroy()
        {
            lock(this)
                return !Running ? Destroy(path) : false;
        }

        public static bool Destroy(string path)
        {
            try
            {
                DB.Destroy(DefaultOptions, path);

                // remove path
                Directory.Delete(path, true);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Repair(string path)
        {
            try
            {
                DB.Repair(DefaultOptions, path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
