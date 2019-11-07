using Bryllite.Cryptography.Signers;
using Bryllite.Extensions;
using Bryllite.Rpc.Web4b;
using Bryllite.Utils.Auth;
using Bryllite.Utils.JsonRpc;
using Bryllite.Utils.NabiLog;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Bryllite.App.Sample.TcpGameServer
{
    public class BrylliteApiService
    {
        public static readonly string LATEST = "latest";
        public static readonly string PENDING = "pending";
        public static readonly string EARLIEST = "earliest";

        public static readonly int POA_TOKEN_REFRESH = 30000;

        // game key
        private readonly PrivateKey gamekey;

        // coinbox
        private readonly string coinbox;

        // cyprus api
        private readonly CyprusApi api;

        public string Coinbase => gamekey.Address;
        public string CoinBox => coinbox;


        public BrylliteApiService(string remote, string gamekey, string coinbox)
        {
            this.gamekey = gamekey;
            this.coinbox = coinbox;

            api = new CyprusApi(remote);
        }

        // user key
        public string GetUserKey(string uid)
        {
            return gamekey.CKD(uid);
        }

        // user address
        public string GetUserAddress(string uid)
        {
            return gamekey.CKD(uid).Address;
        }

        // access token
        public string GetPoAToken(string uid, string hash, string iv)
        {
            try
            {
                // hash & iv
                byte[] hashbytes = Hex.ToByteArray(hash);
                byte[] ivbytes = Hex.ToByteArray(iv);

                // user address & salt
                byte[] address = gamekey.CKD(uid).Address;
                string salt = hashbytes.Append(ivbytes).Append(address).ToHexString();

                // access token
                return new BAuth(token).GetAccessToken(salt);
            }
            finally
            {
                var elapsed = DateTime.Now - tokenTime;
                if (elapsed.TotalMilliseconds >= POA_TOKEN_REFRESH)
                {
                    Task.Run(async () =>
                    {
                        await UpdatePoATokenAsync();
                    });
                }
            }
        }

        private DateTime tokenTime = new DateTime(0);
        private string token = Hex.ToString(SecureRandom.GetBytes(32));
        private async Task UpdatePoATokenAsync()
        {
            try
            {
                string signature = gamekey.Sign(Hex.ToByteArray(token));
                JsonRpc response = await api.PostAsync(new JsonRpc.Request("poa.seed", 0, token, signature));
                token = response.Result<string>(0);
            }
            catch(Exception ex)
            {
                Log.Warning("exception! ex=", ex);
            }
        }

        public void Connect()
        {
            try
            {
                var task = UpdatePoATokenAsync();
            }
            catch(Exception ex)
            {
                Log.Warning("exception! ex=", ex);
            }
        }

        // get user balance
        public async Task<ulong> GetBalanceAsync(string address)
        {
            try
            {
                return await api.GetBalanceAsync(address, LATEST) ?? 0;
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex=", ex);
                return 0;
            }
        }

        public async Task<ulong> GetNonceAsync(string address, bool pending)
        {
            try
            {
                return await api.GetTransactionCountAsync(address, pending ? PENDING : LATEST) ?? 0;
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex=", ex);
                return 0;
            }
        }

        // 내부 이체
        public async Task<string> TransferAsync(string signer, string to, decimal value, decimal gas, ulong? nonce = null)
        {
            try
            {
                return await api.TransferAsync(signer, to, value, gas, nonce);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex=", ex);
                return null;
            }
        }

        // 외부 출금 이체
        public async Task<string> PayoutAsync(string signer, string to, decimal value, decimal gas, ulong? nonce = null)
        {
            try
            {
                return await api.PayoutAsync(signer, to, value, gas, nonce);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex=", ex);
                return null;
            }
        }

        // 트랜잭션 처리 결과를 얻는다
        public async Task<JObject> GetTransactionReceiptAsync(string txid)
        {
            try
            {
                return await api.GetTransactionReceiptAsync(txid);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex=", ex);
                return null;
            }
        }

        // 트랜잭션이 처리될때까지 대기한다
        public async Task<string> WaitForTransactionConfirm(string txid, int timeout = 0)
        {
            try
            {
                var sw = Stopwatch.StartNew();

                JObject receipt = null;
                while (receipt == null)
                {
                    if (timeout > 0 && sw.ElapsedMilliseconds >= timeout)
                        break;

                    await Task.Delay(100);

                    receipt = await GetTransactionReceiptAsync(txid);
                }

                return receipt?.Value<string>("blockHash");
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex=", ex);
                return null;
            }
        }

    }
}
