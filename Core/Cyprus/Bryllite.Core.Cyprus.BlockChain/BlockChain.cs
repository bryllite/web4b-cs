using Bryllite.Core.Accounts;
using Bryllite.Core.Cyprus.States;
using Bryllite.Cryptography.Hash.Extensions;
using Bryllite.Cryptography.Signers;
using Bryllite.Database.Trie;
using Bryllite.Database.TrieDB;
using Bryllite.Utils.NabiLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bryllite.Core.Cyprus
{
    public partial class BlockChain : IDisposable
    {
        public static readonly string PATH = "data";
        public static readonly string STATEDB = "statedb";
        public static readonly string CHAINDB = "chaindb";

        // genesis block
        private readonly Genesis genesis;

        // db base path
        private readonly string path;

        // chaindb
        private ITrieDB chaindb;
        // statedb
        private ITrieDB statedb;
        public ITrieDB ChainDB => chaindb;
        public ITrieDB StateDB => statedb;

        // is running?
        public bool Running;

        // new block accepted event handler
        public event EventHandler<Block> OnNewBlockAccepted;


        public BlockChain(Genesis genesis) : this(genesis, PATH)
        {
        }

        public BlockChain(Genesis genesis, string path)
        {
            this.genesis = genesis;
            this.path = path;

            // chaindb & statedb
            chaindb = new FileDB(GetChainDBPath());
            statedb = new FileDB(GetStateDBPath());
        }

        public string GetStateDBPath()
        {
            return Path.Combine(path, STATEDB);
        }

        public string GetChainDBPath()
        {
            return Path.Combine(path, CHAINDB);
        }

        public void Dispose()
        {
            statedb.Stop();
            statedb = null;

            chaindb.Stop();
            chaindb = null;
        }

        // start block chain
        public void Start()
        {
            if (Running) return;
            Running = true;

            // chaindb & statedb
            chaindb.Start();
            statedb.Start();

            // 저장된 최신 블록 해시
            latest = chaindb.Get(LATEST_KEY);
            if (ReferenceEquals(latest, null))
            {
                // 제네시스 블록 기록
                Write(genesis.Block);

                // 제네시스 상태 저장
                genesis.InitializeTo(statedb);
            }

            // 0번 블록과 제네시스 블록 비교
            var earliest = GetBlock(0);
            if (!genesis.Block.Rlp.HashEquals(earliest.Rlp))
                Log.Panic("genesis block corrupted!");
        }

        // stop
        public void Stop()
        {
            if (!Running) return;
            Running = false;

            Dispose();
        }

        // 이 블록을 실행했을 때의 state root 값을 구한다.
        // 만약 실행할 수 없는 경우 null을 반환한다
        public H256 Try(Block block)
        {
            if (!CanExecutable(block))
                return null;

            // state
            using (var state = new StateMachine(statedb, Latest.StateRoot))
                return state.Run(block);
        }

        // 블록을 실행하고 상태 변화를 저장한다
        public H256 Run(Block block)
        {
            lock (this)
            {
                if (!CanExecutable(block))
                    return null;

                // state
                using (var state = new StateMachine(statedb, Latest.StateRoot))
                {
                    // run block and check state root
                    if (state.Run(block) != block.StateRoot)
                        return null;

                    // 상태 저장
                    state.Commit();

                    // 블록 저장
                    Write(block);

                    // new block accepted event
                    OnNewBlockAccepted?.Invoke(this, block);

                    return state.RootHash;
                }
            }
        }

        // 현재 상태에서 이 블록을 실행할 수 있는지 확인한다
        public bool CanExecutable(Block block)
        {
            // 블록 넘버가 연속되는가?
            if (Latest.Number + 1 != block.Number)
                return false;

            // 새 블록의 이전 블록 해시가 현재 블록 해시와 일치하는가?
            if (Latest.Hash != block.ParentHash)
                return false;

            // 트랜잭션 루트 해시 검사
            if (block.TransactionRoot != Block.ToMerkleRoot(block))
                return false;

            return true;
        }

        // account를 얻는다
        public Account GetAccount(Address address, H256 stateRoot)
        {
            using (var state = new StateMachine(statedb, stateRoot))
                return state.GetAccount(address);
        }

        public Account GetAccount(Address address)
        {
            return GetAccount(address, Latest.StateRoot);
        }

        public Account GetAccount(Address address, long number)
        {
            return GetAccount(address, GetHeader(number).StateRoot);
        }

        // account 목록을 얻는다
        public IDictionary<Address, Account> ToDictionary(H256 stateRoot)
        {
            Dictionary<Address, Account> accounts = new Dictionary<Address, Account>();

            using (var trie = new Trie(statedb, stateRoot))
            {
                foreach (var entry in trie)
                    accounts.Add(entry.Key, Account.TryParse(entry.Value, out Account account) ? account : new Account());

                return accounts;
            }
        }

        // 최신 account 목록을 얻는다
        public IDictionary<Address, Account> ToDictionary()
        {
            return ToDictionary(Latest.StateRoot);
        }


        // 주어진 블록의 DB 무결성을 검증한다
        public bool Check(H256 hash)
        {
            try
            {
                // 블록 & 헤더
                var block = GetBlock(hash);
                var header = block.Header;

                // 트랜잭션 루트 체크
                if (header.TransactionRoot != Block.ToMerkleRoot(block))
                    return false;

                // 상태 루트 체크
                if (!StateChecker.Check(statedb, header.StateRoot))
                    return false;

                // 트랜잭션 lookup entry
                foreach (var tx in block)
                    if (ReferenceEquals(GetTx(tx.Txid), null))
                        return false;

                // 이전 헤더 검사
                return GetHeader(header.ParentHash).Number + 1 == header.Number;
            }
            catch
            {
                return false;
            }
        }


        // repair db ( should not running )
        public bool Repair()
        {
            if (Running) return false;

            try
            {
                FileDB.Repair(GetStateDBPath());
                FileDB.Repair(GetChainDBPath());

                return true;
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message);
                return false;
            }
        }

        // drop db ( should not running )
        public bool Drop()
        {
            if (Running) return false;

            try
            {
                FileDB.Destroy(GetStateDBPath());
                FileDB.Destroy(GetChainDBPath());

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
