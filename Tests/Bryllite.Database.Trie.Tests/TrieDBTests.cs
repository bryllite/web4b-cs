using Bryllite.Database.TrieDB;
using System;
using System.Collections.Generic;
using Xunit;

namespace Bryllite.Database.Trie.Tests
{
    public class TrieDBTests
    {
        [Theory]
        [InlineData(100000)]
        public void FileDBShouldBeEnumerable(int repeats)
        {
            const string path = "filedb/enum";

            // delete existing db
            FileDB.Destroy(path);

            // filedb
            var db = new FileDB(path, true);

            // ·£´ýÅ° ¹ë·ù »ý¼º
            Dictionary<byte[], byte[]> expected = new Dictionary<byte[], byte[]>(new ByteArrayComparer());
            for (int i = 0; i < repeats; i++)
                expected.Add(SecureRandom.GetBytes(32), SecureRandom.GetBytes(64));

            // insert into db
            foreach (var kv in expected)
                db.Put(kv.Key, kv.Value);

            // enumerate
            int items = 0;
            foreach (var kv in db.AsEnumerable())
            {
                byte[] key = kv.Key;
                byte[] value = kv.Value;

                Assert.True(expected.ContainsKey(key));
                Assert.Equal(expected[key], value);

                items++;
            }

            Assert.Equal(expected.Count, items);
        }

        [Theory]
        [InlineData(100000)]
        public void MemoryDBShouldBeEnumerable(int repeats)
        {
            // memdb
            var db = new MemoryDB();

            // ·£´ýÅ° ¹ë·ù »ý¼º
            Dictionary<byte[], byte[]> expected = new Dictionary<byte[], byte[]>(new ByteArrayComparer());
            for (int i = 0; i < repeats; i++)
                expected.Add(SecureRandom.GetBytes(32), SecureRandom.GetBytes(64));

            // insert to db
            foreach (var kv in expected)
                db.Put(kv.Key, kv.Value);

            // enumerate
            int items = 0;
            foreach (var kv in db.AsEnumerable())
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
}
