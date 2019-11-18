using Bryllite.Cryptography.Hash.Extensions;
using Bryllite.Database.Trie;
using Bryllite.Database.TrieDB;
using Bryllite.Utils.NabiLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Core.Cyprus.States
{
    public class StateChecker
    {
        public static bool Check(ITrieDB statedb, H256 root)
        {
            using (var trie = new Trie(statedb, root))
                return Check(statedb, trie, root);
        }

        public static bool Check(ITrieDB statedb, ITrie trie, H256 hash)
        {
            if (ReferenceEquals(hash, null)) return false;

            try
            {
                // get node rlp
                var rlp = statedb.Get(hash);

                // node rlp check
                if (rlp?.Hash256() != hash) return false;

                var node = new TrieNode(trie, rlp);
                if (node.Type == NodeType.FullNode)
                {
                    for (byte radix = 0; radix < 16; radix++)
                    {
                        var child = node.GetChild(radix);
                        if (!ReferenceEquals(child, null) && !Check(statedb, trie, child.Hash))
                            return false;
                    }
                }
                else if (node.Type == NodeType.ShortNode)
                    return Check(statedb, trie, node.Next.Hash);

                return true;
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message);
                return false;
            }
        }
    }
}
