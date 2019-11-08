using Bryllite.Extensions;
using LevelDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bryllite.App.Sample.TcpGameServer
{
    public class Database : IDisposable, IEnumerable<KeyValuePair<byte[], byte[]>>
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

        public bool Running => !ReferenceEquals(db, null);

        public Database(string path) : this(path, DefaultOptions, DefaultEncoding)
        {
        }

        public Database(string path, bool start) : this(path, DefaultOptions, DefaultEncoding)
        {
            if (start) Start();
        }

        public Database(string path, Options options) : this(path, options, DefaultEncoding)
        {
        }

        public Database(string path, Options options, Encoding encoding)
        {
            this.path = path;
            this.options = options;
            this.encoding = encoding;
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            return db?.GetEnumerator();
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
                    db.Close();
                    db = null;
                }
            }
        }

        public void Start()
        {
            Guard.Assert(db == null, "already started");

            path.MakeSureDirectoryPathExists();
            db = new DB(options, path, encoding);
        }

        public void Stop()
        {
            Dispose();
        }

        public byte[] Get(byte[] key)
        {
            if (db == null || key.IsNullOrEmpty()) return null;

            lock (db)
                return db.Get(key);
        }

        public bool TryGet(byte[] key, out byte[] value)
        {
            try
            {
                lock (db)
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
            if (db == null || key.IsNullOrEmpty()) return false;

            lock (db)
                db.Put(key, value);

            return true;
        }

        public bool Del(byte[] key)
        {
            if (db == null || key.IsNullOrEmpty()) return false;

            lock (db)
                db.Delete(key);

            return true;
        }

        public bool Has(byte[] key)
        {
            return !ReferenceEquals(Get(key), null);
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
