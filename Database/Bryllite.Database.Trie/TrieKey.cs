using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bryllite.Extensions;

namespace Bryllite.Database.Trie
{
    public class TrieKey
    {
        public static readonly TrieKey EmptyKey = new TrieKey() { HasTerm = true, Path = new Nibble[0] };

        // 단말노드인가?
        public bool HasTerm;

        // 경로
        public Nibble[] Path;

        // 경로 길이
        public int Length => Path.Length;

        public bool Empty => Length == 0;

        // 경로 인덱서
        public Nibble this[int idx] => Path[idx];

        public byte[] RLP => Encode();

        public TrieKey()
        {
            Path = new Nibble[0];
            HasTerm = true;
        }

        public TrieKey(Nibble[] path, bool terminal)
        {
            HasTerm = terminal;
            Path = path.ToArray();
        }

        public TrieKey(TrieKey other)
        {
            Path = other.Path.ToArray();
            HasTerm = other.HasTerm;
        }

        public TrieKey Clone()
        {
            return new TrieKey(this);
        }

        public Nibble Pop()
        {
            Nibble nibble = Nibble.Null;

            if (Length > 0)
            {
                nibble = this[Length - 1];
                Path = Path.Take(Length - 1).ToArray();
            }

            return nibble;
        }

        public Nibble[] Pop(int count)
        {
            List<Nibble> nibbles = new List<Nibble>();

            for (int i = 0; i < count; i++)
            {
                Nibble nibble = Pop();
                if (!nibble.IsNull) nibbles.Add(nibble);
            }

            return nibbles.ToArray();
        }

        public Nibble PopFront()
        {
            Nibble nibble = Nibble.Null;
            if (Length > 0)
            {
                nibble = this[0];
                Path = Path.Skip(1).ToArray();
            }

            return nibble;
        }

        public Nibble[] PopFront(int count)
        {
            List<Nibble> nibbles = new List<Nibble>();

            for (int i = 0; i < count; i++)
            {
                Nibble nibble = PopFront();
                if (!nibble.IsNull) nibbles.Add(nibble);
            }

            return nibbles.ToArray();
        }

        public void Push(Nibble nibble)
        {
            Path = Path.Append(nibble);
        }

        public void Push(Nibble[] nibbles)
        {
            Path = Path.Append(nibbles);
        }

        public void Push(TrieKey key)
        {
            Path = Path.Append(key?.Path);
        }


        public TrieKey Skip(int count)
        {
            return new TrieKey(Path.Skip(Math.Max(0, count)).ToArray(), HasTerm);
        }

        public TrieKey Take(int count)
        {
            return new TrieKey(Path.Take(Math.Max(0, count)).ToArray(), HasTerm);
        }

        // 앞에서 부터 count만큼 잘라내 버린다.
        public TrieKey TrimLeft(int count)
        {
            return Skip(Math.Max(0, count));
        }

        // 끝에서 부터 count만큼 잘라낸다.
        public TrieKey TrimRight(int count)
        {
            return Take(Math.Max(0, Length - count));
        }

        public int Compare(TrieKey key)
        {
            return !ReferenceEquals(key, null) ? CountMatchingNibbleLength(Path, key.Path) : 0;
        }

        public int Compare(TrieKey key, out TrieKey keyRemain)
        {
            int count = Compare(key);
            keyRemain = Skip(count);
            return count;
        }

        public int Compare(TrieKey key, out TrieKey keyMatch, out TrieKey keyRemain)
        {
            int count = Compare(key);

            keyMatch = Take(count);
            keyRemain = Skip(count);

            return count;
        }

        // 일치하는 키를 구한다.
        public TrieKey GetMatchKey(TrieKey key)
        {
            int count = Compare(key);
            return Take(count);
        }

        // 일치하는 키와 남은 키를 구한다.
        public TrieKey GetMatchKey(TrieKey key, out TrieKey keyRemain)
        {
            int count = Compare(key);
            keyRemain = Skip(count);
            return Take(count);
        }


        // 키를 HexPrefix 포함된 RLP로 인코딩한다.
        public byte[] Encode()
        {
            List<Nibble> encoder = new List<Nibble>();

            encoder.AddRange(Path.Length % 2 == 0 ? new Nibble[] { 0, 0 } : new Nibble[] { 1 });
            encoder.AddRange(Path);

            if (HasTerm) encoder[0] += 2;
            return encoder.ToArray().ToByteArray();
        }

        // HexPrefix 포함된 RLP를 키로 디코딩한다.
        public static TrieKey Decode(byte[] key)
        {
            if (ReferenceEquals(key, null)) return EmptyKey;
            Nibble[] nibbles = key.ToNibbleArray();

            return new TrieKey()
            {
                Path = nibbles.Skip(nibbles[0] % 2 == 0 ? 2 : 1).ToArray(),
                HasTerm = nibbles[0] > 1
            };
        }

        public static implicit operator byte[] (TrieKey key)
        {
            return key.Encode();
        }

        public override string ToString()
        {
            return Path.ToHexString();
        }

        public override int GetHashCode()
        {
            return RLP.ToHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as TrieKey;
            return !ReferenceEquals(o, null) && Path.SequenceEqual(o.Path);
        }

        public static bool operator ==(TrieKey left, TrieKey right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(TrieKey left, TrieKey right)
        {
            return !(left == right);
        }

        public static TrieKey operator +(TrieKey left, TrieKey right)
        {
            var key = new TrieKey();
            key.Push(left);
            key.Push(right);
            return key;
        }

        public static int CountMatchingNibbleLength(Nibble[] left, Nibble[] right)
        {
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return 0;

            int length = Math.Min(left.Length, right.Length);
            for (int i = 0; i < length; i++)
                if (left[i] != right[i]) return i;

            return length;
        }
    }
}
