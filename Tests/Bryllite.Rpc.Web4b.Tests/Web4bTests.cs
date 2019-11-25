using Bryllite.Cryptography.Signers;
using Bryllite.Utils.Currency;
using Bryllite.Utils.Ntp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bryllite.Rpc.Web4b.Tests
{
    public class Web4bTests
    {
        public static readonly string RemoteUrl = "ws://15.164.14.248:4742";
        //public static readonly string RemoteUrl = "ws://localhost:4742";

        private Be4Api be4 = new Be4Api(RemoteUrl);
        private CyprusApi cyprus = new CyprusApi(RemoteUrl);

        private PrivateKey nodeKey = "0x71807c6849611ea301bd79e53e73bc43835ba7c12c5a819014e8b1d0f575b3a4";
        private string coinbase => nodeKey.Address;

        static Web4bTests()
        {
            NetTime.Synchronize();
        }

        [Fact]
        public async Task ShouldWeb4bApiResponsible()
        {
            // web4b_getVersion
            Assert.Equal("Web4b/" + Version.Ver, await be4.GetVersionAsync());

            // web4b_getTime
            var timestamp = await be4.GetTimeAsync();
            Assert.NotNull(timestamp);
            Assert.True(Math.Abs(NetTime.UnixTimeInMs - timestamp.Value) <= 1000);

            // web4b_sha3
            Assert.Equal("0xc5d2460186f7233c927e7db2dcc703c0e500b653ca82273b7bfad8045d85a470", await be4.GetSha3Async(""));
            Assert.Equal("0x47173285a8d7341e5e972fc677286384f802f8ef42a5ec5f03bbfa254cb01fad", await be4.GetSha3Async("0x68656c6c6f20776f726c64"));

            // web4b_mining
            var mining = await be4.GetMiningAsync();
            Assert.NotNull(mining);
        }

        [Fact]
        public async Task ShouldBe4ApiResponsible()
        {
            // be4_coinbase
            string coinbase = await be4.GetCoinbaseAsync();
            Assert.Equal(coinbase, coinbase);

            // be4_getBalance
            var balance = await be4.GetBalanceAsync(coinbase, "latest");
            Assert.NotNull(balance);
            Assert.NotNull(await be4.GetBalanceAsync(coinbase, "pending"));
            Assert.NotNull(await be4.GetBalanceAsync(coinbase, "earliest"));
            Assert.NotNull(await be4.GetBalanceAsync(coinbase, "0x00"));

            // be4_getTransactionCount
            var nonce = await be4.GetTransactionCountAsync(coinbase, "latest");
            Assert.NotNull(nonce);
            Assert.NotNull(await be4.GetTransactionCountAsync(coinbase, "pending"));
            Assert.NotNull(await be4.GetTransactionCountAsync(coinbase, "earliest"));
            Assert.NotNull(await be4.GetTransactionCountAsync(coinbase, "0x00"));

            // be4_blockNumber
            var blockNumber = await be4.GetBlockNumberAsync();
            Assert.NotNull(blockNumber);

            // be4_getBlockByNumber
            var block = await be4.GetBlockByNumberAsync(blockNumber.Value, true);
            Assert.NotNull(block);
            Assert.Equal(Hex.ToString(blockNumber.Value, true), block.Value<string>("number"));
            H256 hash = block.Value<string>("hash");
            Assert.NotNull(hash);

            // be4_getBlockByHash
            var b = await be4.GetBlockByHashAsync(hash, true);
            Assert.NotNull(b);
            Assert.Equal(blockNumber, Hex.ToNumber<long>(b.Value<string>("number")));

            // be4_getBlockTransactionCountByHash
            var count = await be4.GetBlockTransactionCountByHash(hash);
            Assert.NotNull(count);
            Assert.Equal(block.Value<JArray>("transactions").Count, count.Value);

            // be4_getBlockTransactionCountByNumber
            count = await be4.GetBlockTransactionCountByNumber(blockNumber.Value);
            Assert.NotNull(count);
            Assert.Equal(block.Value<JArray>("transactions").Count, count.Value);

        }

        [Fact]
        public async Task ShouldBe4ApiSendTransaction()
        {
            // should coinbase has at least 1 brc 
            // or send transaction test meaningless
            var balance = await be4.GetBalanceAsync(coinbase, "latest");
            Assert.True(balance.Value > Coin.ToBeryl(1));

            // should mining is running
            Assert.True(await be4.GetMiningAsync());

            // random to
            Address receiver = SecureRandom.GetBytes(20);
            ulong amount = Coin.ToBeryl(0.001m);

            // be4_sendRawTransaction & be4_getTransactionReceipt
            var tx = await be4.SendTransactionAndWaitReceiptAsync(nodeKey, receiver, amount, 0);
            var blockHash = tx.Value<string>("blockHash");
            var blockNumber = Hex.ToNumber<long>(tx.Value<string>("blockNumber"));
            var transactionIndex = Hex.ToNumber<int>(tx.Value<string>("transactionIndex"));
            var txid = tx.Value<string>("hash");
            var chain = tx.Value<string>("chain");
            var timestamp = tx.Value<string>("timestamp");
            var from = tx.Value<string>("from");
            var to = tx.Value<string>("to");
            var value = Hex.ToNumber<ulong>(tx.Value<string>("value"));
            var gas = Hex.ToNumber<ulong>(tx.Value<string>("gas"));
            var nonce = Hex.ToNumber<ulong>(tx.Value<string>("nonce"));

            Assert.Equal(coinbase, from);
            Assert.Equal(receiver, to);
            Assert.Equal(amount, value);

            // be4_getTransactionByHash
            var tx1 = await be4.GetTransactionByHashAsync(txid);
            Assert.Equal(txid, tx1.Value<string>("hash"));

            // be4_getTransactionByBlockHashAndIndex
            var tx2 = await be4.GetTransactionByBlockHashAndIndex(blockHash, transactionIndex);
            Assert.Equal(txid, tx2.Value<string>("hash"));

            // be4_getTransactionByBlockNumberAndIndex
            var tx3 = await be4.GetTransactionByBlockNumberAndIndex(blockNumber, transactionIndex);
            Assert.Equal(txid, tx3.Value<string>("hash"));

            // be4_getTransactionsByAddress
            var txs = await be4.GetTransactionsByAddressAsync(coinbase);
            Assert.True(txs.Count > 0);
        }
    }
}
