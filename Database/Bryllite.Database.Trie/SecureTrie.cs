using Bryllite.Cryptography.Hash.Extensions;
using Bryllite.Database.TrieDB;
using Bryllite.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Database.Trie
{
    public class SecureTrie : Trie
    {
        public SecureTrie() : base()
        {
        }

        public SecureTrie(ITrieDB db) : base(db)
        {
        }

        public SecureTrie(ITrieDB db, H256 rootHash) : base(db, rootHash)
        {
        }

        protected override TrieKey GetTrieKey(byte[] key)
        {
            return new TrieKey(key.Hash256().ToNibbleArray(), true);
        }
    }
}
