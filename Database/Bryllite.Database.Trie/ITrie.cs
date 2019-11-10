using System;
using System.Collections.Generic;

namespace Bryllite.Database.Trie
{
    public interface ITrie : IDisposable, IEnumerable<KeyValuePair<byte[], byte[]>>
    {
        // trie root hash
        H256 RootHash { get; }

        // trie interface ( has, get, put, del, commit )
        bool Has(byte[] key);
        byte[] Get(byte[] key);
        void Put(byte[] key, byte[] value);
        void Del(byte[] key);
        H256 Commit();

        // DB에 기록하거나 DB에서 읽는다.
        void Write(byte[] rlp);
        byte[] Read(H256 key);
    }
}
