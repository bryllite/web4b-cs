using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Bryllite.Cryptography.Hash.Extensions;
using Bryllite.Database.TrieDB;

namespace Bryllite.Database.Trie.Tests
{
    public class TrieTests
    {
        /// <summary>
        /// 입력된 항목이 동일하면 입력 순서에 상관없이 항상 동일한 RootHash 값을 가져야 한다.
        /// 랜덤하게 생성한 키를 정순/역순/랜덤 순으로 입력하여 루트 해시값이 동일한지 확인한다.
        /// </summary>
        [Theory]
        [InlineData(10000)]
        public void TrieRootShouldBeEqualOnUnorderedInsert(int repeats)
        {
            // 랜덤 키/밸류 생성
            Dictionary<byte[], byte[]> KeyValues = new Dictionary<byte[], byte[]>(new ByteArrayComparer());
            for (int i = 0; i < repeats; i++)
                KeyValues.Add(SecureRandom.GetBytes(32), SecureRandom.GetBytes(SecureRandom.Next(1, 65)));

            // 1. 정순으로 입력
            ITrie trie1 = new Trie();
            foreach (var kv in KeyValues)
                trie1.Put(kv.Key, kv.Value);

            // 입력 값 확인
            foreach (var kv in KeyValues)
                Assert.Equal(kv.Value, trie1.Get(kv.Key));

            // 2. 역순으로 입력
            ITrie trie2 = new Trie();
            foreach (var kv in KeyValues.Reverse())
                trie2.Put(kv.Key, kv.Value);

            // 입력 값 확인
            foreach (var kv in KeyValues)
                Assert.Equal(kv.Value, trie2.Get(kv.Key));

            // trie1 == trie2
            Assert.Equal(trie1.RootHash, trie2.RootHash);

            // 3. 랜덤순으로 입력
            ITrie trie3 = new Trie();
            foreach (var kv in KeyValues.OrderBy(x => SecureRandom.Next<int>()))
                trie3.Put(kv.Key, kv.Value);

            // 입력 값 확인
            foreach (var kv in KeyValues)
                Assert.Equal(kv.Value, trie3.Get(kv.Key));

            // trie1 == trie3 
            Assert.Equal(trie1.RootHash, trie3.RootHash);
        }

        /// <summary>
        /// 키의 길이가 서로 다르더라도 정상적으로 동작해야 한다.
        /// 단, 키의 길이는 1 이상이어야 한다.
        /// </summary>
        [Theory]
        [InlineData(10000)]
        public void TrieShouldWorksWithAnyKeyLength(int repeats)
        {
            // 랜덤 키 생성 : 키의 길이는 1 - 64
            List<byte[]> keys = new List<byte[]>();
            for (int i = 0; i < repeats; i++)
                keys.Add(SecureRandom.GetBytes(SecureRandom.Next(1, 65)));

            // 1. 정순으로 입력
            ITrie trie = new Trie();
            for (int i = 0; i < keys.Count; i++)
                trie.Put(keys[i], keys[i].Hash256());

            // 1. 정순 입력값 확인
            for (int i = 0; i < keys.Count; i++)
                Assert.Equal(keys[i].Hash256(), trie.Get(keys[i]));

            // 2. 역순으로 입력
            ITrie trie2 = new Trie();
            for (int i = keys.Count - 1; i >= 0; --i)
                trie2.Put(keys[i], keys[i].Hash256());

            // 2. 역순 입력값 확인
            for (int i = 0; i < keys.Count; i++)
                Assert.Equal(keys[i].Hash256(), trie2.Get(keys[i]));

            // 2. trie1 == trie2 
            Assert.Equal(trie.RootHash, trie2.RootHash);
        }

        /// <summary>
        /// BaseTrie는 주어진 byte[] key 를 그대로 이용한다.
        /// SecureTrie는 주어진 byte[] key를 SHA3 해시하여 키로 이용한다.
        /// SecureTrie 상태로 사용하더라도 정상 동작해야 한다.
        /// </summary>
        [Theory]
        [InlineData(10000)]
        public void SecureTrieShouldWorks(int repeats)
        {
            // 랜덤 키 생성 : 키의 길이는 1 - 64
            List<byte[]> keys = new List<byte[]>();
            for (int i = 0; i < repeats; i++)
                keys.Add(SecureRandom.GetBytes(SecureRandom.Next(1, 65)));

            // 1. 정순으로 입력
            ITrie trie = new SecureTrie();
            for (int i = 0; i < keys.Count; i++)
                trie.Put(keys[i], keys[i].Hash256());

            // 1. 정순 입력값 확인
            for (int i = 0; i < keys.Count; i++)
                Assert.Equal(keys[i].Hash256(), trie.Get(keys[i]));

            // 2. 역순으로 입력
            ITrie trie2 = new SecureTrie();
            for (int i = keys.Count - 1; i >= 0; --i)
                trie2.Put(keys[i], keys[i].Hash256());

            // 2. 역순 입력값 확인
            for (int i = 0; i < keys.Count; i++)
                Assert.Equal(keys[i].Hash256(), trie2.Get(keys[i]));

            // 2. trie1 == trie2 
            Assert.Equal(trie.RootHash, trie2.RootHash);
        }

        /// <summary>
        /// BaseTrie, SecureTrie 는 LevelDB를 이용한 FileDB에서 동작해야 한다.
        /// </summary>
        [Theory]
        [InlineData(10000)]
        public void TrieShouldWorksWithLevelDB(int repeats)
        {
            const string path = "data";

            // leveldb
            FileDB.Destroy(path);
            FileDB db = new FileDB(path, true);

            // 랜덤 키 생성 : 키의 길이는 1 - 64
            List<byte[]> keys = new List<byte[]>();
            for (int i = 0; i < repeats; i++)
                keys.Add(SecureRandom.GetBytes(SecureRandom.Next(1, 65)));

            // for BaseTrie
            {
                ITrie trie = new Trie(db);
                foreach (var key in keys)
                    trie.Put(key, key.Hash256());

                H256 root = trie.Commit();
                trie.Dispose();

                ITrie trie2 = new Trie(db, root);
                foreach (var key in keys)
                    Assert.Equal(key.Hash256(), trie2.Get(key));
            }

            // for SecureTrie
            {
                ITrie trie = new SecureTrie(db);
                foreach (var key in keys)
                    trie.Put(key, key.Hash256());

                H256 root = trie.Commit();
                trie.Dispose();

                ITrie trie2 = new SecureTrie(db, root);
                foreach (var key in keys)
                    Assert.Equal(key.Hash256(), trie2.Get(key));
            }
        }

        // trie enumerable
        [Theory]
        [InlineData(10000)]
        public void MemoryTrieShouldBeEnumerable(int repeats)
        {
            // 랜덤키 밸류 생성
            Dictionary<byte[], byte[]> expected = new Dictionary<byte[], byte[]>(new ByteArrayComparer());
            for (int i = 0; i < repeats; i++)
                expected.Add(SecureRandom.GetBytes(32), SecureRandom.GetBytes(64));

            // insert into trie
            using (var trie = new Trie())
            {
                foreach (var kv in expected)
                    trie.Put(kv.Key, kv.Value);

                // enumerate
                int items = 0;
                foreach (var kv in trie.AsEnumerable())
                {
                    byte[] key = kv.Key;
                    byte[] value = kv.Value;

                    Assert.True(expected.ContainsKey(key));
                    Assert.Equal(expected[key], value);

                    items++;
                }

                Assert.Equal(expected.Count, items);
            }
        }

        // trie enumerable
        [Theory]
        [InlineData(10000)]
        public void FileTrieShouldBeEnumerable(int repeats)
        {
            // 랜덤키 밸류 생성
            Dictionary<byte[], byte[]> expected = new Dictionary<byte[], byte[]>(new ByteArrayComparer());
            for (int i = 0; i < repeats; i++)
                expected.Add(SecureRandom.GetBytes(32), SecureRandom.GetBytes(64));

            // leveldb
            const string path = "filedb/enum";
            FileDB.Destroy(path);
            FileDB db = new FileDB(path, true);

            // insert into trie
            using (var trie = new Trie(db))
            {
                foreach (var kv in expected)
                    trie.Put(kv.Key, kv.Value);

                // enumerate
                int items = 0;
                foreach (var kv in trie.AsEnumerable())
                {
                    byte[] key = kv.Key;
                    byte[] value = kv.Value;

                    Assert.True(expected.ContainsKey(key));
                    Assert.Equal(expected[key], value);

                    items++;
                }

                Assert.Equal(expected.Count, items);
            }
        }

        /// <summary>
        /// trie는 update, delete가 가능해야 한다.
        /// </summary>
        [Theory]
        [InlineData(10000)]
        public void TrieShouldUpdatable(int repeats)
        {
            const string path = "data1";

            // leveldb
            FileDB.Destroy(path);
            FileDB db = new FileDB(path, true);

            // 랜덤 키 생성 : 키의 길이는 1 - 64
            List<byte[]> keys = new List<byte[]>();
            for (int i = 0; i < repeats; i++)
                keys.Add(SecureRandom.GetBytes(SecureRandom.Next(1, 65)));

            ITrie trie = new Trie(db);

            // put
            foreach (var key in keys)
                trie.Put(key, key.Hash256());

            // update
            foreach (var key in keys)
                trie.Put(key, key.Hash512());

            H256 root = trie.Commit();
            trie.Dispose();

            // update된 내용이 맞는지 확인
            trie = new Trie(db, root);
            foreach (var key in keys)
                Assert.Equal(key.Hash512(), trie.Get(key));

            // 모든 키 삭제
            foreach (var key in keys)
                trie.Del(key);

            root = trie.Commit();
            trie.Dispose();

            // 키에 해당하는 내용이 있는지 확인
            trie = new Trie(db, root);
            foreach (var key in keys)
                Assert.Null(trie.Get(key));
        }

    }
}
