using Bryllite.Cryptography.Hash.Extensions;
using Bryllite.Utils.Rlp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bryllite.Database.Trie
{
    /// <summary>
    /// trie node type
    /// </summary>
    public enum NodeType
    {
        FullNode,   // branch node
        ShortNode,  // extension node
        ValueNode,  // leaf node
        EmptyNode   // empty node
    }

    /// <summary>
    /// trie node
    /// </summary>
    public class TrieNode : IDisposable
    {
        // node type
        public NodeType Type { get; private set; }

        // node hash
        public H256 Hash => rlp?.Hash256();

        // parent trie
        private readonly ITrie trie;

        // raw rlp data
        private byte[] rlp;

        // 노드 키 
        // 리프 노드 ( Can be Empty )
        // 익스텐션 노드 ( NonEmpty )
        private TrieKey key;

        // 밸류
        // 브랜치 및 리프 일때 유효
        private byte[] value;

        // 익스텐션 노드의 자식 노드
        private TrieNode next;

        // 브랜치의 자식 노드 ( 16개 )
        private TrieNode[] chidrens;

        // is parsed node?
        public bool Parsed;

        // has modified?
        public bool Dirty;

        // new node by rlp
        public TrieNode(ITrie trie, byte[] rlp)
        {
            this.trie = trie;
            this.rlp = rlp.ToArray();

            Type = GetNodeType(this.rlp);
        }

        // new empty branch node
        public TrieNode(ITrie trie)
        {
            this.trie = trie;
            rlp = RlpEncoder.New(17).Encode();

            Type = NodeType.FullNode;
            Dirty = true;

            DecodeRLP();
        }

        // new leaf node
        public TrieNode(ITrie trie, TrieKey key, byte[] value)
        {
            this.trie = trie;
            rlp = new RlpEncoder(key, value).Encode();

            Type = NodeType.ValueNode;
            Dirty = true;

            DecodeRLP();
        }

        // new extension node
        public TrieNode(ITrie trie, TrieKey key, TrieNode next)
        {
            this.trie = trie;
            rlp = new RlpEncoder(key, next.Hash).Encode();

            Type = NodeType.ShortNode;
            Dirty = true;

            DecodeRLP();
        }

        // 노드 키
        public TrieKey Key
        {
            get
            {
                if (Type != NodeType.ShortNode && Type != NodeType.ValueNode) throw new Exception("can't access key");

                DecodeRLP();
                return key.Clone();
            }
            set
            {
                key = value.Clone();
                Dirty = true;
            }
        }

        // 노드 밸류
        public byte[] Value
        {
            get
            {
                if (Type != NodeType.FullNode && Type != NodeType.ValueNode) throw new Exception("can't access value");

                DecodeRLP();
                return value?.ToArray();
            }
            set
            {
                this.value = value?.ToArray();
                Dirty = true;
            }
        }

        // 익스텐션 차일드 노드
        public TrieNode Next
        {
            get
            {
                if (Type != NodeType.ShortNode) throw new Exception("can't access next node");

                DecodeRLP();
                return next;
            }
            set
            {
                next = value;
                Dirty = true;
            }
        }

        // 브랜치 노드의 차일드 노드
        public TrieNode GetChild(Nibble radix)
        {
            if (Type != NodeType.FullNode) throw new Exception("can't access child node");

            DecodeRLP();
            return chidrens[radix];
        }

        // 브랜치 노드의 차일드 노드
        public void SetChild(Nibble radix, TrieNode node)
        {
            if (Type != NodeType.FullNode) throw new Exception("can't access child node");

            DecodeRLP();
            chidrens[radix] = node;
            Dirty = true;
        }

        // 노드를 RLP로 인코딩한다.
        internal TrieNode EncodeRLP()
        {
            if (Parsed || Dirty)
            {
                // 브랜치 노드 인코딩
                if (Type == NodeType.FullNode)
                {
                    var encoder = new RlpEncoder();
                    for (int i = 0; i < 16; i++)
                        encoder.Add(chidrens[i]?.Hash);
                    encoder.Add(value);
                    rlp = encoder.Encode();
                }
                else if (Type == NodeType.ShortNode)
                {
                    // 익스텐션 노드 인코딩
                    rlp = new RlpEncoder(Key, Next.Hash).Encode();
                }
                else if (Type == NodeType.ValueNode)
                {
                    // 리프 노드 인코딩
                    rlp = new RlpEncoder(Key, Value).Encode();
                }
                else throw new Exception("can't encode unknown node type");
            }

            return this;
        }

        // RLP를 디코딩하여 노드 상태를 완전하게 만든다.
        internal TrieNode DecodeRLP()
        {
            if (ReferenceEquals(rlp, null)) throw new Exception("can't decode empty rlp");

            if (!Parsed)
            {
                var rlp = new RlpDecoder(this.rlp);

                if (Type == NodeType.FullNode)
                {
                    if (rlp.Count != 17) throw new Exception("can't decode rlp for full node");

                    // child node
                    chidrens = new TrieNode[16];
                    for (int i = 0; i < 16; i++)
                    {
                        byte[] node = trie.Read(rlp[i].Value);
                        chidrens[i] = node != null ? new TrieNode(trie, node) : null;
                    }

                    // value
                    value = rlp[16].Value;
                }
                else if (Type == NodeType.ShortNode)
                {
                    if (rlp.Count != 2) throw new Exception("can't decode rlp for short node");

                    // key
                    key = TrieKey.Decode(rlp[0].Value);

                    // next node
                    byte[] node = trie.Read(rlp[1].Value);
                    next = node != null ? new TrieNode(trie, node) : null;
                }
                else if (Type == NodeType.ValueNode)
                {
                    if (rlp.Count != 2) throw new Exception("can't decode rlp for value node");

                    // key
                    key = TrieKey.Decode(rlp[0].Value);

                    // value
                    value = rlp[1].Value;
                }
                else
                {
                    throw new Exception("can't decoded rlp for unknown node type");
                }

                Parsed = true;
            }

            return this;
        }

        public override string ToString()
        {
            return Hash;
        }

        public void Commit()
        {
            if (Dirty)
            {
                Dirty = false;

                EncodeRLP();
                trie.Write(rlp);
            }
        }

        public void Dispose()
        {
            rlp = null;
            key = null;
            next = null;
            chidrens = null;
            value = null;
        }

        public static NodeType GetNodeType(byte[] rlp)
        {
            return GetNodeType(new RlpDecoder(rlp));
        }

        public static NodeType GetNodeType(RlpDecoder rlp)
        {
            if (rlp.Count == 17) return NodeType.FullNode;
            if (rlp.Count == 2) return TrieKey.Decode(rlp[0].Value).HasTerm ? NodeType.ValueNode : NodeType.ShortNode;
            return NodeType.EmptyNode;
        }

    }
}
