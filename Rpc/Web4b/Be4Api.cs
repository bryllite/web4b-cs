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
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b
{
    public class Be4Api
    {
        // api provider
        private IWeb4bProvider provider;

        public Be4Api(IWeb4bProvider provider)
        {
            this.provider = provider;
        }

        public Be4Api(Uri remote)
        {
            if ("http" == remote.Scheme.ToLower())
                provider = new HttpProvider(remote);
            else if ("ws" == remote.Scheme.ToLower())
                provider = new WebSocketProvider(remote);
            else throw new ArgumentException("unsupported uri scheme");
        }

        public Be4Api(string remote) : this(new Uri(remote))
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
        /// method: be4_blockNumber
        /// params: none
        /// result: block number
        /// </summary>
        /// <returns></returns>
        public async Task<long> BlockNumberAsync()
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("be4_blockNumber", 0));
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
        /// method: be4_getBalance
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
                response = await PostAsync(new JsonRpc.Request("be4_getBalance", 0, address, tag));
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
        /// method: be4_getTransactionCount
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
                response = await PostAsync(new JsonRpc.Request("be4_getTransactionCount", 0, address, tag));
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
        /// method: be4_getTransactionByHash
        /// params: txid(hex)
        /// result: transaction object
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async Task<JObject> GetTransactionByHashAsync(string txid)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("be4_getTransactionByHash", 0, txid));
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
        /// method: be4_getTransactionByBlockNumberAndIndex
        /// params: block number, tx index
        /// result: transaction object
        /// </summary>
        /// <param name="number"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<JObject> GetTransactionByBlockNumberAndIndex(long number, int index)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("be4_getTransactionByBlockNumberAndIndex", 0, number.ToHexString(true), index.ToHexString(true)));
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
        /// method: be4_getTransactionByBlockHashAndIndex
        /// params: block hash, tx index
        /// result: transaction object
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<JObject> GetTransactionByBlockHashAndIndex(string hash, int index)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("be4_getTransactionByBlockHashAndIndex", 0, hash, index.ToHexString(true)));
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
        /// method: be4_sendRawTransaction
        /// params: transaction rlp(hex)
        /// result: txid
        /// </summary>
        /// <param name="tx"></param>
        /// <returns></returns>
        public async Task<string> SendRawTransactionAsync(string tx)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("be4_sendRawTransaction", 0, tx));
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
        /// method: be4_sendRawTransaction
        /// params: signer key, to, value, gas, nonce
        /// result: txid
        /// </summary>
        /// <param name="signer"></param>
        /// <param name="to"></param>
        /// <param name="value"></param>
        /// <param name="gas"></param>
        /// <param name="nonce"></param>
        /// <returns></returns>
        public async Task<string> SendTransactionAsync(PrivateKey signer, string to, decimal value, decimal gas, ulong nonce)
        {
            return await SendRawTransactionAsync(CreateTransactionAsync(signer, to, value, gas, nonce));
        }

        public async Task<string> SendTransactionAsync(string signer, string to, decimal value, decimal gas, ulong nonce)
        {
            return await SendTransactionAsync(new PrivateKey(signer), to, value, gas, nonce);
        }

        public async Task<string> SendTransactionAsync(PrivateKey signer, string to, decimal value, decimal gas)
        {
            ulong? nonce = await GetTransactionCountAsync(signer.Address.Hex, "pending");
            if (nonce == null) return "";

            return await SendTransactionAsync(signer, to, value, gas, nonce.Value);
        }

        public async Task<string> SendTransactionAsync(string signer, string to, decimal value, decimal gas)
        {
            return await SendTransactionAsync(new PrivateKey(signer), to, value, gas);
        }

        public string CreateTransactionAsync(PrivateKey signer, string to, decimal value, decimal gas, ulong nonce)
        {
            // tx
            return new Tx()
            {
                To = to,
                Value = Coin.ToBeryl(value),
                Gas = Coin.ToBeryl(gas),
                Nonce = nonce
            }.Sign(signer).Rlp.ToHexString();
        }

        public string CreateTransactionAsync(string signer, string to, decimal value, decimal gas, ulong nonce)
        {
            return CreateTransactionAsync(new PrivateKey(signer), to, value, gas, nonce);
        }

        public async Task<string> CreateTransactionAsync(PrivateKey signer, string to, decimal value, decimal gas)
        {
            ulong? nonce = await GetTransactionCountAsync(signer.Address.Hex, "pending");
            if (nonce == null) return string.Empty;

            return CreateTransactionAsync(signer, to, value, gas, nonce.Value);
        }

        public async Task<string> CreateTransactionAsync(string signer, string to, decimal value, decimal gas)
        {
            return await CreateTransactionAsync(new PrivateKey(signer), to, value, gas);
        }


        /// <summary>
        /// method: be4_pendingTransactions
        /// params: none
        /// result: array of pending tx
        /// </summary>
        /// <returns></returns>
        public async Task<JArray> PendingTransactionAsync()
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("be4_pendingTransactions", 0));
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
        /// method: be4_getTransactionReceipt
        /// params: txid
        /// result: transaction receipt object
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async Task<JObject> GetTransactionReceiptAsync(string txid)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("be4_getTransactionReceipt", 0, txid));
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
        /// method: be4_getBlockByHash
        /// params: block hash, fulltx?
        /// result: block object
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public async Task<JObject> GetBlockByHash(string hash, bool tx)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("be4_getBlockByHash", 0, hash, Hex.ToString(tx, true)));
                if (response.HasError)
                    Log.Warning("error has occured! error=", response.ErrorMessage);

                return response.Result<JObject>(0);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", resonse=", response);
                return null;
            }
        }

        /// <summary>
        /// method: be4_getBlockByNumber
        /// params: block number, fulltx?
        /// result: block object
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public async Task<JObject> GetBlockByNumber(long number, bool tx)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("be4_getBlockByNumber", 0, Hex.ToString(number, true), Hex.ToString(tx, true)));
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

        public async Task<string> GetBlockRlpByNumber(long number)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("be4_getBlockByNumber", 0, Hex.ToString(number, true), Hex.ToString(0x02, true)));
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
        /// method: be4_getBlockTransactionCountByHash
        /// params: block hash
        /// result: transaction count
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public async Task<int> GetBlockTransactionCountByHash(string hash)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("be4_getBlockTransactionCountByHash", 0, hash));
                if (response.HasError)
                    Log.Warning("error has occured! erorr=", response.ErrorMessage);

                return response.Result<string>(0).ToNumber<int>();
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return -1;
            }
        }

        /// <summary>
        /// method: be4_getBlockTransactionCountByNumber
        /// params: block number
        /// result: transaction count
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public async Task<int> GetBlockTransactionCountByNumber(long number)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("be4_getBlockTransactionCountByNumber", 0, number.ToHexString()));
                if (response.HasError)
                    Log.Warning("error has occured! erorr=", response.ErrorMessage);

                return response.Result<string>(0).ToNumber<int>();
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", response=", response);
                return -1;
            }
        }


        public async Task<IEnumerable<JObject>> GetTransactionsByAddressAsync(string address)
        {
            var txs = new List<JObject>();

            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("be4_getTransactionsByAddress", 0, address, true));
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
