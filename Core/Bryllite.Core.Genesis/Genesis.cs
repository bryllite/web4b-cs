using Bryllite.Core.Accounts;
using Bryllite.Cryptography.Signers;
using Bryllite.Database.Trie;
using Bryllite.Database.TrieDB;
using Bryllite.Utils.NabiLog;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bryllite.Core
{
    public class Genesis
    {
        // genesis block
        private Block genesis;
        public Block Block => genesis;

        // genesis block header
        public BlockHeader Header => Block.Header;

        // genesis block chain id
        public byte Chain => Header.Chain;

        // genesis block hash
        public H256 Hash => Block.Hash;

        // genesis block coinbase
        public Address Coinbase => Header.Coinbase;

        // genesis block state root
        public H256 StateRoot => Header.StateRoot;

        // genesis block transaction root
        public H256 TransactionRoot => Header.TransactionRoot;

        // genesis accounts
        private Dictionary<Address, Account> accounts = new Dictionary<Address, Account>();


        public Genesis(string file)
        {
            Create(file);
        }

        public void Create(string file)
        {
            try
            {
                Create(JObject.Parse(File.ReadAllText(file)));
            }
            catch (Exception ex)
            {
                Log.Panic("exception! ex.Message=", ex.Message);
            }
        }

        private void Create(JObject json)
        {
            try
            {
                // genesis block header
                var header = json["header"];
                Hex chain = header.Value<string>("chain");
                Hex version = header.Value<string>("version");
                Hex reserved = header.Value<string>("reserved");
                Hex timestamp = header.Value<string>("timestamp");
                Hex number = header.Value<string>("number");
                Hex coinbase = header.Value<string>("coinbase");
                Hex witness = header.Value<string>("witness");
                Hex nonce = header.Value<string>("nonce");
                Hex difficulty = header.Value<string>("difficulty");
                Hex parent = header.Value<string>("parent");
                Hex txroot = header.Value<string>("txroot");
                Hex data = header.Value<string>("data");
                Hex extra = header.Value<string>("extra");

                // genesis block accounts
                foreach (var entry in JArray.FromObject(json["accounts"]))
                {
                    Address address = entry.Value<string>("address");
                    Hex balance = entry.Value<string>("balance");
                    accounts[address] = new Account() { Balance = balance };
                }

                // genesis state root
                H256 stateRoot = GetRootState();


                // create genesis block
                genesis = new Block(new BlockHeader()
                {
                    Chain = chain,
                    Version = version,
                    Reserved = reserved,
                    Timestamp = timestamp,
                    Number = number,
                    Coinbase = coinbase,
                    Witness = witness,
                    Nonce = nonce,
                    Difficulty = difficulty,
                    ParentHash = (byte[])parent,
                    TransactionRoot = (byte[])txroot,
                    StateRoot = stateRoot,
                    Data = data,
                    Extra = extra,
                });
            }
            catch (Exception ex)
            {
                Log.Panic("exception! ex.Message=", ex.Message);
            }
        }

        // root state 값을 얻는다.
        public H256 GetRootState()
        {
            return InitializeTo(new MemoryDB());
        }

        // account 목록의 상태를 DB에 초기화하고 root state 를 리턴한다
        public H256 InitializeTo(ITrieDB db)
        {
            return InitializeTo(db, accounts);
        }

        // account 목록의 상태를 DB에 초기화하고 root state 를 리턴한다
        public static H256 InitializeTo(ITrieDB db, IDictionary<Address, Account> accounts)
        {
            try
            {
                using (var trie = new Trie(db))
                {
                    foreach (var entry in accounts)
                    {
                        Address address = entry.Key;
                        Account account = entry.Value;

                        trie.Put(address, account.Rlp);
                    }

                    return trie.Commit();
                }
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message);
                return null;
            }
        }

    }
}
