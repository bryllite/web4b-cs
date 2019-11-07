using Bryllite.Rpc.Web4b.Providers;
using Bryllite.Utils.JsonRpc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b
{
    public class CyprusApiV2
    {
        // web4b
        private readonly IWeb4bProvider web4b;

        // exceptions
        private List<string> errors = new List<string>();

        public CyprusApiV2(IWeb4bProvider provider)
        {
            web4b = provider;
        }

        public CyprusApiV2(Uri remote)
        {
            if ("http" == remote.Scheme.ToLower())
                web4b = new HttpProvider(remote);
            else if ("ws" == remote.Scheme.ToLower())
                web4b = new WebSocketProvider(remote);
            else throw new ArgumentException("unsupported uri scheme");
        }

        public CyprusApiV2(string remote) : this(new Uri(remote))
        {
        }

        public string GetLastError()
        {
            lock (errors)
                return errors.Last();
        }

        public void SetLastError(string error)
        {
            lock (errors)
                errors.Add(error);
        }

        public IEnumerable<string> GetAllErrors()
        {
            lock (errors)
                return errors.ToArray();
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


        public async Task<string> GetVersionAsync( int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("web4b_getVersion", id));
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

        public async Task<long?> GetTimeAsync( int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("web4b_getTime", id));
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

        public async Task<string> Sha3Async(string hex, int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("web4b_sha3", id, hex));
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

        public async Task<string> CoinbaseAsync(int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("web4b_coinbase", id));
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

        public async Task<bool?> MiningAsync(int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("web4b_mining", id));
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

        public async Task<long?> BlockNumberAsync(int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_blockNumber", id));
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

        public async Task<long?> GetBalanceAsync(string address, string number, int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getBalance", id, address, number));
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

        public async Task<long?> GetBalanceByUidAsync(string uid, string number, int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getBalanceByUid", id, uid, number));
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

        public async Task<long?> GetTransactionCountAsync(string address, string number, int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionCount", id, address, number));
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

        public async Task<long?> GetTransactionCountByUidAsync(string uid, string number, int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionCountByUid", id, uid, number));
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

        public async Task<int?> GetBlockTransactionCountByHashAsync(string hash, int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getBlockTransactionCountByHash", id, hash));
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

        public async Task<int?> GetBlockTransactionCountByNumberAsync(long number, int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getBlockTransactionCountByHash", id, Hex.ToString(number, true)));
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

        public async Task<string> SendRawTransactionAsync(string tx, int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_sendRawTransaction", id, tx));
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

        public async Task<JObject> GetBlockByHashAsync(string hash, int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getBlockByHash", id, hash));
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

        public async Task<JObject> GetBlockByNumberAsync(long number, int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getBlockByNumber", id, number));
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

        public async Task<JObject> GetTransactionByHashAsync(string txid, int id = 0)
        {
            JsonRpc response = null;
            try
            {
                response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionByHash", id, txid));
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


    }
}
