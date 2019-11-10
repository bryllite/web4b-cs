using System;
using System.Collections.Generic;

namespace Bryllite.Database.TrieDB
{
    /// <summary>
    /// trie db
    /// key / value db
    /// </summary>
    public interface ITrieDB : IDisposable, IEnumerable<KeyValuePair<byte[], byte[]>>
    {
        IEnumerable<byte[]> Keys { get; }
        IEnumerable<byte[]> Values { get; }

        // is db running?
        bool Running { get; }

        // start & stop
        void Start();
        void Stop();

        // get
        byte[] Get(byte[] key);
        bool TryGet(byte[] key, out byte[] value);


        // put
        bool Put(byte[] key, byte[] value);

        // del
        bool Del(byte[] key);

        // has key?
        bool Has(byte[] key);

    }
}
