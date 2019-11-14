using Bryllite.Cryptography.Signers;
using Bryllite.Utils.Currency;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Bryllite.Rpc.Web4b.Tests
{
    public class Be4ApiTests
    {
        private static readonly PrivateKey key = "0x71807c6849611ea301bd79e53e73bc43835ba7c12c5a819014e8b1d0f575b3a4";
        private static readonly string coinbase = key.Address;

        private Be4Api api = new Be4Api("ws://localhost:4742");

        [Fact]
        public async Task GetVersionAsyncTest()
        {
            string version = await api.GetVersionAsync();
            Assert.NotNull(version);
        }

        [Fact]
        public async Task GetTimeAsyncTest()
        {
            long? time = await api.GetTimeAsync();
            Assert.NotNull(time);
        }

        [Theory]
        [InlineData("", "0xc5d2460186f7233c927e7db2dcc703c0e500b653ca82273b7bfad8045d85a470")]
        [InlineData("0x68656c6c6f20776f726c64", "0x47173285a8d7341e5e972fc677286384f802f8ef42a5ec5f03bbfa254cb01fad")]
        public async Task GetSha3AsyncTest(string message, string expected)
        {
            string actual = await api.GetSha3Async(message);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetMiningAsyncTest()
        {
            bool? mining = await api.GetMiningAsync();
            Assert.NotNull(mining);
        }

        [Fact]
        public async Task GetCoinbaseAsyncTest()
        {
            string expected = coinbase;
            string actual = await api.GetCoinbaseAsync();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetBlockNumberAsyncTest()
        {
            long? number = await api.GetBlockNumberAsync();
            Assert.True(number.Value >= 0);
        }

        [Theory]
        [InlineData("earliest")]
        [InlineData("latest")]
        [InlineData("pending")]
        [InlineData("0x00")]
        public async Task GetBalanceAsyncTest(string number)
        {
            ulong? actual = await api.GetBalanceAsync(coinbase, number);
            Assert.NotNull(actual);
        }

        [Theory]
        [InlineData("earliest")]
        [InlineData("latest")]
        [InlineData("pending")]
        [InlineData("0x00")]
        public async Task GetTransactionCountAsyncTest(string number)
        {
            ulong? actual = await api.GetTransactionCountAsync(coinbase, number);
            Assert.NotNull(actual);
        }

        [Fact]
        public async Task GetPendingTransactionsAsyncTest()
        {
            JArray txs = await api.GetPendingTransactionsAsync();
            Assert.NotNull(txs);
        }

        [Fact]
        public async Task TransactionsTest()
        {
            string signer = key;
            string to = "0x1a11a57397b129bc56743d9324782e715925c1aa";
            decimal value = 0.1m;

            // make tx and wait for receipt
            // SendTransactionAsync(), SendRawTransactionAsync(), GetTransactionReceiptAsync()
            var receipt = await api.SendTransactionAndWaitReceiptAsync(signer, to, Coin.ToBeryl(value));
            Assert.NotNull(receipt);

            long blockNumber = Hex.ToNumber<long>(receipt.Value<string>("blockNumber"));
            string blockHash = receipt.Value<string>("blockHash");
            int transactionIndex = Hex.ToNumber<int>(receipt.Value<string>("transactionIndex"));
            string txid = receipt.Value<string>("hash");

            // GetTransactionByHashAsync()
            var tx = await api.GetTransactionByHashAsync(txid);
            Assert.Equal(txid, tx.Value<string>("hash"));

            // GetTransactionByBlockHashAndIndexAsync()
            tx = await api.GetTransactionByBlockHashAndIndex(blockHash, transactionIndex);
            Assert.Equal(txid, tx.Value<string>("hash"));

            // GetTransactionByBlockNumberAndIndexAsync()
            tx = await api.GetTransactionByBlockNumberAndIndex(blockNumber, transactionIndex);
            Assert.Equal(txid, tx.Value<string>("hash"));
        }

        [Fact]
        public async Task GetTransactionsByAddressAsyncTest()
        {
            var txs = await api.GetTransactionsByAddressAsync(coinbase, true);
            Assert.NotNull(txs);
        }

        [Fact]
        public async Task BlockTests()
        {
            long? latest = await api.GetBlockNumberAsync();

            Assert.NotNull(latest);
            long number = latest.Value;

            // GetBlockByNumberAsync()
            var block = await api.GetBlockByNumberAsync(number, number % 2 == 0);
            Assert.NotNull(block);

            string blockHash = block.Value<string>("hash");
            long blockNumber = Hex.ToNumber<long>(block.Value<string>("number"));

            Assert.True(!string.IsNullOrEmpty(blockHash));
            Assert.Equal(number, blockNumber);

            // GetBlockByHashAsync()
            var block2 = await api.GetBlockByHashAsync(blockHash, number % 2 != 0);

            Assert.NotNull(block2);
            Assert.Equal(number, Hex.ToNumber<long>(block2.Value<string>("number")));

            // GetBlockRlpAsync()
            var rlp = await api.GetBlockRlpAsync(number);
            Assert.True(!string.IsNullOrEmpty(rlp));

            // GetBlockTransactionCountByHashAsync()
            int? txs = await api.GetBlockTransactionCountByHash(blockHash);
            Assert.True(txs.Value >= 0);

            // GetBlockTransactionCountByNumberAsync()
            txs = await api.GetBlockTransactionCountByNumber(number);
            Assert.True(txs.Value >= 0);
        }
    }
}
