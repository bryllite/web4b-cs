using Bryllite.Cryptography.Signers;
using Bryllite.Rpc.Web4b.Be4;
using Bryllite.Rpc.Web4b.Cyprus;
using Bryllite.Utils.Currency;
using Bryllite.Utils.Ntp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bryllite.Rpc.Web4b.Tests
{
    public class Web4bTests
    {
        public static readonly string Web4bUrl = "ws://localhost:9627";

        private Be4Helper be4 = new Be4Helper(Web4bUrl);
        private CyprusHelper cyprus = new CyprusHelper(Web4bUrl);

        static Web4bTests()
        {
            NetTime.Synchronize();
        }

        [Fact]
        public async Task ShouldPassWeb4bCallTest()
        {
            // web4b_getTime
            {
                (long unixtime, string error) = await be4.GetTimeAsync();
                Assert.True(string.IsNullOrEmpty(error));
                Assert.True(Math.Abs(NetTime.Timestamp - unixtime) < 1000);
            }

            // web4b_getVersion
            {
                (string actual, string error) = await be4.GetVersionAsync();
                Assert.Equal("web4b/" + Version.Ver, actual);
            }

            // web4b_sha3
            {
                (string actual, string error) = await be4.GetSha3Async("");
                Assert.Equal("0xc5d2460186f7233c927e7db2dcc703c0e500b653ca82273b7bfad8045d85a470", actual);
                (actual, error) = await be4.GetSha3Async("0x68656c6c6f20776f726c64");
                Assert.Equal("0x47173285a8d7341e5e972fc677286384f802f8ef42a5ec5f03bbfa254cb01fad", actual);
            }

            // web4b_mining
            {
                (bool? mining, string error) = await be4.GetMiningAsync();
                Assert.NotNull(mining);
                Assert.True(string.IsNullOrEmpty(error));
            }

        }

        [Fact]
        public async Task ShouldPassBe4CallTest()
        {
            string error;
            Address coinbase;
            long number;
            H256 hash;
            ulong? balance, nonce;
            JObject block, block1;
            int count;

            // be4_coinbase
            (coinbase, error) = await be4.GetCoinbaseAsync();
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(string.IsNullOrEmpty(coinbase));

            // be4_blockNumber
            (number, error) = await be4.GetBlockNumberAsync();
            Assert.True(string.IsNullOrEmpty(error));

            // be4_getBalance ( earliest )
            (balance, error) = await be4.GetBalanceAsync(coinbase, Be4Helper.EARLIEST);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(balance);

            // be4_getBalance ( latest )
            (balance, error) = await be4.GetBalanceAsync(coinbase, Be4Helper.LATEST);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(balance);

            // be4_getBalance ( pending )
            (balance, error) = await be4.GetBalanceAsync(coinbase, Be4Helper.PENDING);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(balance);

            // be4_getBalance ( number )
            (balance, error) = await be4.GetBalanceAsync(coinbase, Hex.ToString(number, true));
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(balance);

            // be4_getTransactionCount ( earliest )
            (nonce, error) = await be4.GetTransactionCountAsync(coinbase, Be4Helper.EARLIEST);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(nonce);

            // be4_getTransactionCount ( latest )
            (nonce, error) = await be4.GetTransactionCountAsync(coinbase, Be4Helper.LATEST);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(nonce);

            // be4_getTransactionCount ( pending )
            (nonce, error) = await be4.GetTransactionCountAsync(coinbase, Be4Helper.PENDING);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(nonce);

            // be4_getBalance ( number )
            (nonce, error) = await be4.GetTransactionCountAsync(coinbase, Hex.ToString(number, true));
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(nonce);

            // be4_getBlockByNumber / be4_getBlockByHash
            (block, error) = await be4.GetBlockByNumberAsync(number, true);
            Assert.True(string.IsNullOrEmpty(error));

            hash = block.Value<string>("hash");

            (block1, error) = await be4.GetBlockByHashAsync(block.Value<string>("hash"), true);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.Equal(block.Value<string>("hash"), block1.Value<string>("hash"));

            // be4_getBlockTransactionCountByHash
            (count, error) = await be4.GetBlockTransactionCountByHashAsync(hash);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.True(count >= 0);

            // be4_getBlockTransactionCountByNumber
            (count, error) = await be4.GetBlockTransactionCountByNumberAsync(number);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.True(count >= 0);

        }

        [Theory]
        [InlineData("0xd0bdd708cba0b518e205b6eb088762afdd340d4c8418c0731767b55e8fc9787c", "0xa07279487ee9f36a9eec7f6b361d9df0b7ec79e9565fd9741d27cdb8adf10425")]
        public async Task ShouldPassBe4TransactionTest(string from, string to)
        {
            PrivateKey sender = from, receiver = to;
            string error, txid;
            bool? mining;
            ulong? balance;
            JArray pending, history;
            JObject receipt, tx;

            // sender should has at leat 1 brc for transaction test
            (balance, error) = await be4.GetBalanceAsync(sender.Address);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(balance);
            Assert.True(balance >= Coin.ToBeryl(1));

            // should mining 
            (mining, error) = await be4.GetMiningAsync();
            Assert.True(string.IsNullOrEmpty(error));
            Assert.True(mining);

            // send transaction
            (txid, error) = await be4.SendTransactionAsync(sender, receiver.Address, Coin.ToBeryl(0.0001m));
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(string.IsNullOrEmpty(txid));

            // pending transaction
            (pending, error) = await be4.GetPendingTransactionsAsync();
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(pending is null);
            Assert.True(pending.ToObject<IEnumerable<JObject>>().Where(t => t.Value<string>("hash") == txid).Count() > 0);

            // get transaction receipt
            (receipt, error) = await be4.WaitTransactionReceiptAsync(txid);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(receipt is null);
            Assert.True(receipt.Value<string>("hash") == txid);

            // get transaction
            (tx, error) = await be4.GetTransactionByHashAsync(txid);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(tx is null);
            Assert.True(tx.Value<string>("hash") == txid);

            // get transaction by block hash and index
            (var tx1, string err1) = await be4.GetTransactionByBlockHashAndIndexAsync(tx.Value<string>("blockHash"), Hex.ToNumber<int>(tx.Value<string>("blockIndex")));
            Assert.True(string.IsNullOrEmpty(err1));
            Assert.False(tx1 is null);
            Assert.True(tx1.Value<string>("hash") == txid);

            // get transaction by block number and index
            (var tx2, string err2) = await be4.GetTransactionByBlockNumberAndIndexAsync(Hex.ToNumber<long>(tx.Value<string>("blockNumber")), Hex.ToNumber<int>(tx.Value<string>("blockIndex")));
            Assert.True(string.IsNullOrEmpty(err2));
            Assert.False(tx2 is null);
            Assert.True(tx2.Value<string>("hash") == txid);

            // get transaction history of sender
            (history, error) = await be4.GetTransactionsByAddressAsync(sender.Address, Hex.ToNumber<long>(tx.Value<string>("blockNumber")), true, 20);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(history is null);
            Assert.True(history.ToObject<IEnumerable<JObject>>().Where(t => t.Value<string>("hash") == txid).Count() > 0);

            // get transaction history of receiver
            (history, error) = await be4.GetTransactionsByAddressAsync(receiver.Address, Hex.ToNumber<long>(tx.Value<string>("blockNumber")), false, 20);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(history is null);
            Assert.True(history.ToObject<IEnumerable<JObject>>().Where(t => t.Value<string>("hash") == txid).Count() > 0);

        }


        [Fact]
        public async Task ShouldPassCyprusCallTest()
        {
            string error;
            Address coinbase;
            long number;
            H256 hash;
            ulong? balance, nonce;
            JObject block, block1;
            int count;

            // cyprus_coinbase
            (coinbase, error) = await cyprus.GetCoinbaseAsync();
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(string.IsNullOrEmpty(coinbase));

            // cyprus_blockNumber
            (number, error) = await cyprus.GetBlockNumberAsync();
            Assert.True(string.IsNullOrEmpty(error));

            // cyprus_getBalance ( earliest )
            (balance, error) = await cyprus.GetBalanceAsync(coinbase, CyprusHelper.EARLIEST);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(balance);

            // cyprus_getBalance ( latest )
            (balance, error) = await cyprus.GetBalanceAsync(coinbase, CyprusHelper.LATEST);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(balance);

            // cyprus_getBalance ( pending )
            (balance, error) = await cyprus.GetBalanceAsync(coinbase, CyprusHelper.PENDING);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(balance);

            // cyprus_getBalance ( number )
            (balance, error) = await cyprus.GetBalanceAsync(coinbase, Hex.ToString(number, true));
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(balance);

            // cyprus_getTransactionCount ( earliest )
            (nonce, error) = await cyprus.GetTransactionCountAsync(coinbase, CyprusHelper.EARLIEST);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(nonce);

            // cyprus_getTransactionCount ( latest )
            (nonce, error) = await cyprus.GetTransactionCountAsync(coinbase, CyprusHelper.LATEST);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(nonce);

            // cyprus_getTransactionCount ( pending )
            (nonce, error) = await cyprus.GetTransactionCountAsync(coinbase, CyprusHelper.PENDING);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(nonce);

            // cyprus_getBalance ( number )
            (nonce, error) = await cyprus.GetTransactionCountAsync(coinbase, Hex.ToString(number, true));
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(nonce);

            // cyprus_getBlockByNumber / cyprus_getBlockByHash
            (block, error) = await cyprus.GetBlockByNumberAsync(number, true);
            Assert.True(string.IsNullOrEmpty(error));

            hash = block.Value<string>("hash");

            (block1, error) = await cyprus.GetBlockByHashAsync(block.Value<string>("hash"), true);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.Equal(block.Value<string>("hash"), block1.Value<string>("hash"));

            // cyprus_getBlockTransactionCountByHash
            (count, error) = await cyprus.GetBlockTransactionCountByHashAsync(hash);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.True(count >= 0);

            // cyprus_getBlockTransactionCountByNumber
            (count, error) = await cyprus.GetBlockTransactionCountByNumberAsync(number);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.True(count >= 0);
        }

        [Theory]
        [InlineData("0x7d11b78630f315cbcaa277343b46dc3aab64ef0513eb78f2cc6d56782c267061", "0xed78d23c0f5b7c06a053e17b0dfe0a982f7e63651c467cb07520014a69180f43")]
        public async Task ShouldPassCyprusTransactionTest(string from, string to)
        {
            PrivateKey sender = from, receiver = to;
            string error, txid;
            bool? mining;
            ulong? balance;
            JObject receipt, tx;
            JArray history;

            // sender should has at leat 1 brc for transaction test
            (balance, error) = await cyprus.GetBalanceAsync(sender.Address);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.NotNull(balance);
            Assert.True(balance >= Coin.ToBeryl(1));

            // should mining 
            (mining, error) = await cyprus.GetMiningAsync();
            Assert.True(string.IsNullOrEmpty(error));
            Assert.True(mining);

            // send transfer
            (txid, error) = await cyprus.SendTransferAsync(sender, receiver.Address, Coin.ToBeryl(0.0001m));
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(string.IsNullOrEmpty(txid));

            // get transaction receipt
            (receipt, error) = await cyprus.WaitTransactionReceiptAsync(txid);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(receipt is null);
            Assert.True(receipt.Value<string>("hash") == txid);

            // get transaction
            (tx, error) = await cyprus.GetTransactionByHashAsync(txid);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(tx is null);
            Assert.True(tx.Value<string>("hash") == txid);

            // get transaction by block hash and index
            (var tx1, string err1) = await cyprus.GetTransactionByBlockHashAndIndexAsync(tx.Value<string>("blockHash"), Hex.ToNumber<int>(tx.Value<string>("blockIndex")));
            Assert.True(string.IsNullOrEmpty(err1));
            Assert.False(tx1 is null);
            Assert.True(tx1.Value<string>("hash") == txid);

            // get transaction by block number and index
            (var tx2, string err2) = await cyprus.GetTransactionByBlockNumberAndIndexAsync(Hex.ToNumber<long>(tx.Value<string>("blockNumber")), Hex.ToNumber<int>(tx.Value<string>("blockIndex")));
            Assert.True(string.IsNullOrEmpty(err2));
            Assert.False(tx2 is null);
            Assert.True(tx2.Value<string>("hash") == txid);

            // get transaction history of sender
            (history, error) = await cyprus.GetTransactionsByAddressAsync(sender.Address, Hex.ToNumber<long>(tx.Value<string>("blockNumber")), true, 20);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(history is null);
            Assert.True(history.ToObject<IEnumerable<JObject>>().Where(t => t.Value<string>("hash") == txid).Count() > 0);

            // get transaction history of receiver
            (history, error) = await cyprus.GetTransactionsByAddressAsync(receiver.Address, Hex.ToNumber<long>(tx.Value<string>("blockNumber")), false, 20);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(history is null);
            Assert.True(history.ToObject<IEnumerable<JObject>>().Where(t => t.Value<string>("hash") == txid).Count() > 0);

            // send withdraw
            (txid, error) = await cyprus.SendWithdrawAsync(sender, sender.Address, Coin.ToBeryl(0.1m));
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(string.IsNullOrEmpty(txid));

            // wait for withdraw confirm
            (receipt, error) = await cyprus.WaitTransactionReceiptAsync(txid);
            Assert.True(string.IsNullOrEmpty(error));
            Assert.False(receipt is null);
            Assert.Equal(receipt.Value<string>("hash"), txid);
        }
    }
}
