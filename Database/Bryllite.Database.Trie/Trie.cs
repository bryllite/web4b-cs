using Bryllite.Cryptography.Hash.Extensions;
using Bryllite.Database.TrieDB;
using Bryllite.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Database.Trie
{
    public class Trie : ITrie
    {
        // 트라이 DB
        protected ITrieDB db;

        // 트라이 오리지널 루트 해시
        protected H256 rootHash;

        // 트라이 루트 노드
        protected TrieNode RootNode;

        // 루트 노드 해시값
        public H256 RootHash => RootNode?.Hash;

        // 루트 노드가 존재하는가?
        protected bool HasRoot => !ReferenceEquals(RootNode, null);


        public Trie() : this(new MemoryDB())
        {
        }

        public Trie(ITrieDB db)
        {
            this.db = db;
        }

        public Trie(ITrieDB db, H256 rootHash) : this(db)
        {
            this.rootHash = rootHash;
            RootNode = GetNodeByHash(rootHash);
        }

        public byte[] Read(H256 hash)
        {
            return !ReferenceEquals(hash, null) ? db.Get(hash) : null;
        }

        public void Write(byte[] rlp)
        {
            db.Put(rlp.Hash256(), rlp);
        }


        // Exists?
        public bool Has(byte[] key)
        {
            if (!HasRoot) return false;
            return FindNode(RootNode, GetTrieKey(key)).found != null;
        }

        // GET
        public byte[] Get(byte[] key)
        {
            if (!HasRoot) return null;

            TrieKey k = GetTrieKey(key);
            TrieNode node = FindNode(RootNode, k).found;

            return node != null ? node.Value : null;
        }

        // PUT
        public void Put(byte[] key, byte[] value)
        {
            TrieKey k = GetTrieKey(key);

            if (!HasRoot)
            {
                RootNode = new TrieNode(this, k, value);
                return;
            }

            // 탐색 진행
            (TrieNode found, TrieKey keyRemainder, Stack<TrieNode> stack) = FindNode(RootNode, k);
            if (found != null)
            {
                // 노드를 찾았으면 값을 설정하고 
                // 스택을 저장한다.
                found.Value = value;
                RootNode = SaveStack(k, stack);
                return;
            }

            // 노드를 추가한다.
            RootNode = InsertNode(k, value, keyRemainder, stack);
        }

        // DELETE
        public void Del(byte[] key)
        {
            TrieKey k = GetTrieKey(key);

            if (HasRoot)
            {
                // 탐색 진행
                (TrieNode found, TrieKey keyRemainder, Stack<TrieNode> stack) = FindNode(RootNode, k);
                if (found != null)
                {
                    // 노드를 찾았으면 값을 삭제하고
                    // 스택을 저장한다.
                    found.Value = null;
                    RootNode = SaveStack(k, stack);
                    return;
                }
            }
        }

        // COMMIT
        public H256 Commit()
        {
            lock (db)
            {
                if (!HasRoot) return null;

                Commit(RootNode);
                return RootHash;
            }
        }

        public void Dispose()
        {
            Dispose(RootNode);
            RootNode = null;
        }

        // 현재 트라이의 모든 키/밸류 쌍을 얻는다.
        public IEnumerable<KeyValuePair<byte[], byte[]>> AsEnumerable()
        {
            var enums = new List<KeyValuePair<byte[], byte[]>>();

            lock (db)
            {
                if (HasRoot)
                    ToList(RootNode, TrieKey.EmptyKey, enums);
            }

            return enums;
        }

        protected TrieNode GetNodeByHash(H256 hash)
        {
            byte[] rlp = Read(hash);
            return rlp.IsNullOrEmpty() ? null : new TrieNode(this, rlp);
        }


        protected virtual TrieKey GetTrieKey(byte[] key)
        {
            return new TrieKey(key.ToNibbleArray(), true);
        }


        protected void Commit(TrieNode node)
        {
            if (ReferenceEquals(node, null) || !node.Dirty) return;

            if (node.Type == NodeType.FullNode)
            {
                for (byte i = 0; i < 16; i++)
                    Commit(node.GetChild(i));
            }
            else if (node.Type == NodeType.ShortNode)
            {
                Commit(node.Next);
            }

            node.Commit();
        }

        protected void Dispose(TrieNode node)
        {
            if (ReferenceEquals(node, null)) return;

            if (node.Parsed)
            {
                if (node.Type == NodeType.FullNode)
                {
                    for (byte i = 0; i < 16; i++)
                        Dispose(node.GetChild(i));
                }
                else if (node.Type == NodeType.ShortNode)
                {
                    Dispose(node.Next);
                }
            }

            node.Dispose();
        }

        // 루트 부터 key에 해당하는 경로를 탐색하여
        // 일치하는 노드, 잔여 경로, 노드 스택(경로에 해당하는) 을 구한다.
        protected (TrieNode found, TrieKey keyRemainder, Stack<TrieNode> stack) FindNode(TrieNode root, TrieKey key)
        {
            if (ReferenceEquals(root, null)) throw new ArgumentNullException(nameof(root));
            if (ReferenceEquals(key, null) || key.Empty) throw new ArgumentNullException(nameof(key));

            // 노드 경로 스택
            Stack<TrieNode> stack = new Stack<TrieNode>();

            // 노드와 검색 경로
            TrieNode node = root;
            TrieKey keyRemainder = key.Clone();

            while (node != null && node.Type != NodeType.EmptyNode)
            {
                // 노드 스택에 노드 추가
                stack.Push(node);

                // 현재 노드가 브랜치 노드이면
                if (node.Type == NodeType.FullNode)
                {
                    // 브랜치 노드에서 경로가 종료되면 탐색 종료
                    if (keyRemainder.Empty) return (node, TrieKey.EmptyKey, stack);

                    // 경로에 해당하는 자식 노드가 없으면 탐색 종료
                    Nibble radix = keyRemainder[0];
                    TrieNode child = node.GetChild(radix);
                    if (child == null)
                        return (null, keyRemainder, stack);

                    // 자식 노드로 탐색 계속 진행
                    node = child;
                    keyRemainder.PopFront();
                    continue;
                }

                // 현재 노드가 익스텐션 노드이면
                if (node.Type == NodeType.ShortNode)
                {
                    // 노드 키 ( 항상 키가 존재해야 한다 )
                    TrieKey k = node.Key;
                    Guard.Assert(!k.Empty);

                    // 익스텐션 키가 일치하지 않으면 탐색 종료
                    int matchingLength = keyRemainder.Compare(k);
                    if (matchingLength != k.Length)
                        return (null, keyRemainder, stack);

                    // 익스텐션 노드의 자식 노드로 계속 탐색 진행
                    //keyRemainder.PopFront(k.Length);
                    keyRemainder = keyRemainder.Skip(k.Length);
                    node = node.Next;
                    continue;
                }

                // 리프 노드
                return keyRemainder == node.Key ? (node, TrieKey.EmptyKey, stack) : (null, keyRemainder, stack);
            }

            throw new Exception("wtf!");
        }

        // keyRemainder는 Empty일 수 있다.
        // key는 Empty일 수 없다.
        // stack은 0보다 크다
        protected TrieNode InsertNode(TrieKey key, byte[] value, TrieKey keyRemainder, Stack<TrieNode> stack)
        {
            Guard.Assert(!key.Empty);
            Guard.Assert(stack.Count > 0);

            // 스택의 최종 노드
            TrieNode node = stack.Pop();

            // 최종 노드가 브랜치 노드인 경우
            if (node.Type == NodeType.FullNode)
            {
                stack.Push(node);

                // 브랜치 노드의 자식 노드로 새로운 리프 노드 추가
                // keyRemainder is not empty always
                stack.Push(new TrieNode(this, keyRemainder.Skip(1), value));
            }
            else
            {
                TrieKey nodeKey = node.Key;
                TrieNode fullNode = new TrieNode(this);

                int matchingLength = keyRemainder.Compare(nodeKey);
                if (matchingLength > 0)
                {
                    // 공유키로 익스텐션 노드 생성
                    TrieKey sharedKey = new TrieKey(keyRemainder.Take(matchingLength).Path, false);
                    stack.Push(new TrieNode(this, sharedKey, fullNode));

                    nodeKey = nodeKey.Skip(matchingLength);
                    keyRemainder = keyRemainder.Skip(matchingLength);
                }

                // 스택에 브랜치 노드 추가
                stack.Push(fullNode);

                // 기존 노드 
                if (!nodeKey.Empty)
                {
                    Nibble branchKey = nodeKey.PopFront();
                    if (nodeKey.Empty && node.Type == NodeType.ShortNode)
                    {
                        // replace extension node to branch node
                        var child = node.Next;
                        fullNode.SetChild(branchKey, child.EncodeRLP());
                    }
                    else
                    {
                        node.Key = nodeKey;
                        fullNode.SetChild(branchKey, node.EncodeRLP());
                    }
                }
                else
                {
                    Guard.Assert(node.Type == NodeType.ValueNode);

                    // 브랜치 노드에 값 설정
                    fullNode.Value = node.Value;
                }

                // 스택에 새로운 리프 노드를 추가한다.
                // 만약, 잔여 키가 비어있으면 브랜치 노드에 값 설정한다.
                if (keyRemainder.Empty)
                {
                    fullNode.Value = value;
                }
                else
                {
                    stack.Push(new TrieNode(this, keyRemainder.Skip(1), value));
                }
            }

            return SaveStack(key, stack);
        }


        // 스택을 정리한다.
        protected TrieNode SaveStack(TrieKey key, Stack<TrieNode> stack)
        {
            TrieNode root = null;
            TrieKey k = key.Clone();

            while (stack.Count > 0)
            {
                TrieNode node = stack.Pop();

                if (node.Type == NodeType.ValueNode)
                {
                    k.Pop(node.Key.Length);
                }
                else if (node.Type == NodeType.ShortNode)
                {
                    k.Pop(node.Key.Length);
                    if (null != root)
                        node.Next = root;
                }
                else if (node.Type == NodeType.FullNode)
                {
                    if (null != root)
                    {
                        Guard.Assert(!k.Empty);
                        node.SetChild(k.Pop(), root);
                    }
                }

                // 노드 RLP 인코딩
                root = node.EncodeRLP();
            }

            return root;
        }

        protected void ToList(TrieNode node, TrieKey key, List<KeyValuePair<byte[], byte[]>> array)
        {
            if (node.Type == NodeType.ValueNode)
            {
                byte[] value = node.Value;
                if (!value.IsNullOrEmpty())
                    array.Add(new KeyValuePair<byte[], byte[]>((key + node.Key).Path.ToByteArray(), value));
            }
            else if (node.Type == NodeType.ShortNode)
            {
                ToList(node.Next, key + node.Key, array);
            }
            else if (node.Type == NodeType.FullNode)
            {
                byte[] value = node.Value;
                if (!value.IsNullOrEmpty())
                    array.Add(new KeyValuePair<byte[], byte[]>(key.Path.ToByteArray(), value));

                for (byte i = 0; i < 16; i++)
                {
                    var n = node.GetChild(i);
                    if (!ReferenceEquals(n, null))
                        ToList(n, key + new TrieKey(new Nibble[] { i }, true), array);
                }
            }
        }
    }
}
