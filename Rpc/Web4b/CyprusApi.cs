using Bryllite.Core;
using Bryllite.Cryptography.Signers;
using Bryllite.Extensions;
using Bryllite.Rpc.Web4b.Providers;
using Bryllite.Utils.Currency;
using Bryllite.Utils.JsonRpc;
using Bryllite.Utils.NabiLog;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b
{
    public class CyprusApi
    {
        public static readonly string LATEST = "latest";
        public static readonly string PENDING = "pending";
        public static readonly string EARLIEST = "earliest";

        // api provider
        private IWeb4bProvider provider;

        public CyprusApi(IWeb4bProvider provider)
        {
            this.provider = provider;
        }

        public CyprusApi(Uri remote)
        {
            if ("http" == remote.Scheme.ToLower())
                provider = new HttpProvider(remote);
            else if ("ws" == remote.Scheme.ToLower())
                provider = new WebSocketProvider(remote);
            else throw new ArgumentException("unsupported uri scheme");
        }

        public CyprusApi(string remote) : this(new Uri(remote))
        {
        }

        // jsonrpc request async
        public async Task<JsonRpc> PostAsync(JsonRpc request)
        {
            return JsonRpc.TryParse(await provider.PostAsync(request.ToString()), out JsonRpc response) ? response : null;
        }

        /// <summary>
        /// method: web4b_getVersion
        /// params: none
        /// result: version(string)
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetVersionAsync()
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("web4b_getVersion", 0));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<string>(0);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return string.Empty;
            }
        }

        /// <summary>
        /// method: web4b_getTime
        /// params: none
        /// result: server utc time ticks
        /// </summary>
        /// <returns></returns>
        public async Task<long?> GetTimeAsync()
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("web4b_getTime", 0));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<string>(0).ToNumber<long>();
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return null;
            }
        }

        /// <summary>
        /// method: web4b_sha3
        /// params: hex(string) to get hash
        /// result: hash(string)
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public async Task<string> Sha3Async(string hex)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("web4b_sha3", 0, hex));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<string>(0);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return string.Empty;
            }
        }


        /// <summary>
        /// method: web4b_coinbase
        /// params: none
        /// result: coinbase address(hex)
        /// </summary>
        /// <returns></returns>
        public async Task<string> CoinbaseAsync()
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("web4b_coinbase", 0));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<string>(0);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return string.Empty;
            }
        }

        /// <summary>
        /// method: cyprus_getAddress
        /// params: uid
        /// result: address
        /// </summary>
        /// <param name="uids"></param>
        /// <returns></returns>
        public async Task<string> GetAddressAsync(string uid)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getAddress", 0, uid));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<string>(0);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return null;
            }
        }


        /// <summary>
        /// method: cyprus_blockNumber
        /// params: none
        /// result: block number
        /// </summary>
        /// <returns></returns>
        public async Task<long> BlockNumberAsync()
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_blockNumber", 0));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<string>(0).ToNumber<long>();
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return -1;
            }
        }


        /// <summary>
        /// method: cyprus_getBalance
        /// params: address(hex), tag(string: "latest", "pending", blockNumber)
        /// result: balance(ulong: in beryl)
        /// </summary>
        /// <param name="address"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public async Task<ulong?> GetBalanceAsync(string address, string tag)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getBalance", 0, address, tag));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<string>(0).ToNumber<ulong>();
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return null;
            }
        }

        /// <summary>
        /// method: cyprus_getTransactionCount
        /// params: address(hex), tag(string: "latest", "pending", blockNumber)
        /// result: account nonce(uint)
        /// </summary>
        /// <param name="address"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public async Task<ulong?> GetTransactionCountAsync(string address, string tag)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionCount", 0, address, tag));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<string>(0).ToNumber<ulong>();
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return null;
            }
        }


        /// <summary>
        /// method: cyprus_sendRawTransaction
        /// params: raw tx ( hex )
        /// result: txid
        /// </summary>
        /// <param name="rawTx"></param>
        /// <returns></returns>
        public async Task<string> SendRawTransactionAsync(string rawTx)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_sendRawTransaction", 0, rawTx));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<string>(0);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return null;
            }
        }

        public async Task<string> TransferAsync(PrivateKey signer, Address to, decimal value, decimal gas, ulong? nonce = null)
        {
            if (null == nonce)
                nonce = await GetTransactionCountAsync(signer.Address, PENDING);

            // cyprus tx
            var tx = new Tx(Tx.Transfer)
            {
                To = to,
                Value = Coin.ToBeryl(value),
                Gas = Coin.ToBeryl(gas),
                Nonce = nonce.Value
            }.Sign(signer);

            // send raw tx
            return await SendRawTransactionAsync(Hex.ToString(tx.Rlp));
        }

        public async Task<string> PayoutAsync(PrivateKey signer, Address to, decimal value, decimal gas, ulong? nonce = null)
        {
            if (null == nonce)
                nonce = await GetTransactionCountAsync(signer.Address, PENDING);

            var tx = new Tx(Tx.Payout)
            {
                To = to,
                Value = Coin.ToBeryl(value),
                Gas = Coin.ToBeryl(gas),
                Nonce = nonce.Value
            }.Sign(signer);

            return await SendRawTransactionAsync(tx.Rlp.ToHexString());
        }

        /// <summary>
        /// method: cyprus_pendingTransactions
        /// params: none
        /// result: list of pending transaction
        /// </summary>
        /// <returns></returns>
        public async Task<JArray> PendingTransactionsAsync()
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_pendingTransactions", 0));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.GetResult();
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return null;
            }
        }


        /// <summary>
        /// method: cyprus_getBlockByHash
        /// params: block hash
        /// result: block object
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public async Task<JObject> GetBlockByHashAsync(string hash)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getBlockByHash", 0, hash));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<JObject>(0);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return null;
            }
        }

        /// <summary>
        /// method: cyprus_getBlockByNumber
        /// params: block number
        /// result: block object
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public async Task<JObject> GetBlockByNumberAsync(long number)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getBlockByNumber", 0, number.ToHexString(true)));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<JObject>(0);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return null;
            }
        }


        /// <summary>
        /// method: cyprus_getTransactionByHash
        /// params: txid
        /// result: tx object
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async Task<JObject> GetTransactionByHashAsync(string txid)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionByHash", 0, txid));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<JObject>(0);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return null;
            }
        }

        /// <summary>
        /// method: cyprus_getTransactionByBlockNumberAndIndex
        /// params: number, index
        /// result: tx object
        /// </summary>
        /// <param name="number"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<JObject> GetTransactionByBlockNumberAndIndexAsync(long number, int index)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionByBlockNumberAndIndex", 0, number.ToHexString(true), index.ToHexString(true)));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<JObject>(0);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return null;
            }
        }

        /// <summary>
        /// method: cyprus_getTransactionByBlockHashAndIndex
        /// params: block hash, index
        /// result: tx object
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<JObject> GetTransactionByBlockHashAndIndexAsync(string hash, int index)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionByBlockHashAndIndex", 0, hash, index.ToHexString(true)));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<JObject>(0);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return null;
            }
        }

        /// <summary>
        /// method: cyprus_getTransactionReceipt
        /// params: txid
        /// result: tx receipt object
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async Task<JObject> GetTransactionReceiptAsync(string txid)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionReceipt", 0, txid));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<JObject>(0);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return null;
            }
        }

        /// <summary>
        /// method: cyprus_getBlockTransactionCountByHash
        /// params: block hash
        /// result: tx count
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public async Task<int?> GetBlockTransactionCountByHashAsync(string hash)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getBlockTransactionCountByHash", 0, hash));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<string>(0).ToNumber<int>();
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return null;
            }
        }

        /// <summary>
        /// method: cyprus_getBlockTransactionCountByNumber
        /// params: block number
        /// result: tx count
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public async Task<int?> GetBlockTransactionCountByNumberAsync(long number)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getBlockTransactionCountByNumber", 0, number.ToHexString()));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<string>(0).ToNumber<int>();
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return null;
            }
        }

        public async Task<IEnumerable<JObject>> GetTransactionsByAddressAsync(string address)
        {
            var txs = new List<JObject>();

            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionsByAddress", 0, address, true));
                if (response.HasError)
                    Log.Warning("error has occured! erorr=", response.ErrorMessage);

                for (int i = 0; i < response.ResultCount; i++)
                {
                    var tx = response.Result<JObject>(0);
                    txs.Add(tx);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
            }
            return txs;
        }

        public async Task<IEnumerable<JObject>> GetTransactionsByUidAsync(string uid)
        {
            var txs = new List<JObject>();

            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionsByUid", 0, uid, true));
                if (response.HasError)
                    Log.Warning("error has occured! erorr=", response.ErrorMessage);

                for (int i = 0; i < response.ResultCount; i++)
                {
                    var tx = response.Result<JObject>(0);
                    txs.Add(tx);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
            }
            return txs;
        }


    }
}
