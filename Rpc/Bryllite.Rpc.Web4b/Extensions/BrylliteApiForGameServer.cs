using Bryllite.Cryptography.Hash.Extensions;
using Bryllite.Cryptography.Signers;
using Bryllite.Extensions;
using Bryllite.Utils.Auth;
using Bryllite.Utils.Currency;
using Bryllite.Utils.JsonRpc;
using Bryllite.Utils.NabiLog;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b.Extensions
{
    public class BrylliteApiForGameServer
    {
        public static readonly int POA_TOKEN_REFRESH = 10000;

        // game server key
        private readonly PrivateKey gamekey;

        // cyprus api
        public readonly CyprusApi Cyprus;

        // coinbase address
        public string Coinbase => gamekey.Address;

        // shop address
        public string ShopAddress { get; private set; }


        public BrylliteApiForGameServer(string remote, string gamekey, string shopAddress)
        {
            this.gamekey = gamekey;
            ShopAddress = shopAddress;

            Cyprus = new CyprusApi(remote);
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

        // to brc
        public decimal ToBrc(ulong beryl)
        {
            return Coin.ToCoin(beryl);
        }

        // to beryl
        public ulong ToBeryl(decimal brc)
        {
            return Coin.ToBeryl(brc);
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
                return new BAuth(seed).GetAccessToken(salt);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex=", ex);
                return string.Empty;
            }
        }

        // poa token seed
        private string seed = Hex.ToString(SecureRandom.GetBytes(32));
        public async Task UpdatePoATokenSeedAsync()
        {
            try
            {
                string signature = gamekey.Sign(Hex.ToByteArray(seed));
                JsonRpc response = await Cyprus.PostAsync(new JsonRpc.Request("poa.seed", 0, seed, signature));
                seed = response.Result<string>(0);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex=", ex);
            }
        }


        // get user balance
        public async Task<ulong> GetBalanceAsync(string address)
        {
            try
            {
                return await Cyprus.GetBalanceAsync(address, CyprusApi.LATEST) ?? 0;
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
                return await Cyprus.GetTransactionCountAsync(address, pending ? CyprusApi.PENDING : CyprusApi.LATEST) ?? 0;
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex=", ex);
                return 0;
            }
        }

        // In-Game Tx
        public async Task<string> TransferAsync(string signer, string to, decimal value, decimal gas = 0, ulong? nonce = null)
        {
            try
            {
                return await Cyprus.TransferAsync(signer, to, ToBeryl(value), ToBeryl(gas), nonce);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex=", ex);
                return null;
            }
        }

        // Ex-Game Tx
        public async Task<string> PayoutAsync(string signer, string to, decimal value, decimal gas = 0, ulong? nonce = null)
        {
            try
            {
                return await Cyprus.PayoutAsync(signer, to, ToBeryl(value), ToBeryl(gas), nonce);
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
                return await Cyprus.GetTransactionReceiptAsync(txid);
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

        // get user key export token
        public async Task<string> GetKeyExportTokenAsync(string uid)
        {
            try
            {
                string signature = gamekey.Sign(Encoding.UTF8.GetBytes(uid).Hash256());
                return await Cyprus.ExportKeyTokenAsync(uid, signature);
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex=", ex);
                return null;
            }
        }
    }
}
