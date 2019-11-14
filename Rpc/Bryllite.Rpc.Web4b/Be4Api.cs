using Bryllite.Core;
using Bryllite.Cryptography.Signers;
using Bryllite.Rpc.Web4b.Providers;
using Bryllite.Utils.JsonRpc;
using Bryllite.Utils.Ntp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b
{
    public class Be4Api
    {
        public static readonly string EARLIEST = "earliest";
        public static readonly string LATEST = "latest";
        public static readonly string PENDING = "pending";

        // api provider
        private IWeb4bProvider web4b;

        // errors
        private List<string> errors = new List<string>();


        public Be4Api(IWeb4bProvider web4b)
        {
            this.web4b = web4b;
        }

        public Be4Api(Uri remote)
        {
            if ("http" == remote.Scheme.ToLower())
                web4b = new HttpProvider(remote);
            else if ("ws" == remote.Scheme.ToLower())
                web4b = new WebSocketProvider(remote);
            else throw new ArgumentException("unsupported uri scheme");
        }

        public Be4Api(string remote) : this(new Uri(remote))
        {
        }

        public string GetLastError()
        {
            lock (errors)
                return errors.Last();
        }

        public IEnumerable<string> Errors()
        {
            lock (errors)
                return errors.ToArray();
        }

        public void SetLastError(string error)
        {
            lock (errors)
                errors.Add(error);
        }


        // jsonrpc request async
        public async Task<JsonRpc> PostAsync(JsonRpc request)
        {
            return JsonRpc.TryParse(await web4b.PostAsync(request.ToString()), out JsonRpc response) ? response : null;
        }

        // jsonrpc batch request
        public async Task<JsonRpc.Batch> PostAsync(JsonRpc.Batch batch)
        {
            return JsonRpc.Batch.TryParse(await web4b.PostAsync(batch.ToString()), out JsonRpc.Batch response) ? response : null;
        }


        public async Task<string> GetVersionAsync(int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("web4b_getVersion", id));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<string>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }


        public async Task<long?> GetTimeAsync(int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("web4b_getTime", id));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return Hex.ToNumber<long>(response.Result<string>(0));

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<string> GetSha3Async(string message, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("web4b_sha3", id, message));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<string>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }


        public async Task<string> GetCoinbaseAsync(int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("web4b_coinbase", id));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<string>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<bool?> GetMiningAsync(int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("web4b_mining", id));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<bool>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<long?> GetBlockNumberAsync(int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_blockNumber", id));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return Hex.ToNumber<long>(response.Result<string>(0));

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<ulong?> GetBalanceAsync(string address, string number, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_getBalance", id, address, number));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return Hex.ToNumber<ulong>(response.Result<string>(0));

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }


        public async Task<ulong?> GetTransactionCountAsync(string address, string number, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_getTransactionCount", id, address, number));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return Hex.ToNumber<ulong>(response.Result<string>(0));

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<JObject> GetTransactionByHashAsync(string txid, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_getTransactionByHash", id, txid));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<JObject>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }


        public async Task<JObject> GetTransactionByBlockNumberAndIndex(long number, int index, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_getTransactionByBlockNumberAndIndex", id, Hex.ToString(number, true), Hex.ToString(index, true)));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<JObject>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }


        public async Task<JObject> GetTransactionByBlockHashAndIndex(string hash, int index, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_getTransactionByBlockHashAndIndex", id, hash, Hex.ToString(index, true)));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<JObject>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<string> SendRawTransactionAsync(string txRlp, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_sendRawTransaction", id, txRlp));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<string>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<Tx> CreateTx(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null)
        {
            try
            {
                // signer key
                PrivateKey key = signer;

                // nonce?
                if (null == nonce)
                    nonce = await GetTransactionCountAsync(key.Address, PENDING);

                // make tx
                return new Tx()
                {
                    Timestamp = NetTime.UnixTime,
                    To = to,
                    Value = value,
                    Gas = gas,
                    Nonce = nonce.Value
                }.Sign(key);
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<string> SendTransactionAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            var tx = await CreateTx(signer, to, value, gas, nonce);
            return !ReferenceEquals(tx, null) ? await SendRawTransactionAsync(Hex.ToString(tx.Rlp), id) : null;
        }


        public async Task<JArray> GetPendingTransactionsAsync(int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_pendingTransactions", id));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.GetResult();

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<JObject> GetTransactionReceiptAsync(string txid, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_getTransactionReceipt", id, txid));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<JObject>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<JObject> SendTransactionAndWaitReceiptAsync(CancellationToken cancellation, string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            var txid = await SendTransactionAsync(signer, to, value, gas, nonce, id);
            if (string.IsNullOrEmpty(txid))
                return null;

            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellation);

                    var receipt = await GetTransactionReceiptAsync(txid, id);
                    if (!ReferenceEquals(receipt, null))
                        return receipt;
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<JObject> SendTransactionAndWaitReceiptAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            return await SendTransactionAndWaitReceiptAsync(CancellationToken.None, signer, to, value, gas, nonce, id);
        }


        public async Task<JObject> GetBlockByHashAsync(string hash, bool tx, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_getBlockByHash", id, hash, Hex.ToString(tx, true)));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<JObject>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }


        public async Task<JObject> GetBlockByNumberAsync(long number, bool tx, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_getBlockByNumber", id, Hex.ToString(number, true), Hex.ToString(tx, true)));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<JObject>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<string> GetBlockRlpAsync(long number, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_getBlockByNumber", id, Hex.ToString(number, true), Hex.ToString(0x02, true)));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<string>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<int?> GetBlockTransactionCountByHash(string hash, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_getBlockTransactionCountByHash", id, hash));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return Hex.ToNumber<int>(response.Result<string>(0));

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<int?> GetBlockTransactionCountByNumber(long number, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_getBlockTransactionCountByNumber", id, Hex.ToString(number, true)));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return Hex.ToNumber<int>(response.Result<string>(0));

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<JArray> GetTransactionsByAddressAsync(string address, bool tx = false, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("be4_getTransactionsByAddress", id, address, tx));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.GetResult() ?? new JArray();

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

    }
}
