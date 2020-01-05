using Bryllite.Core;
using Bryllite.Cryptography.Hash.Extensions;
using Bryllite.Cryptography.Signers;
using Bryllite.Extensions;
using Bryllite.Rpc.Web4b.Cyprus;
using Bryllite.Utils.Auth;
using Bryllite.Utils.Currency;
using Bryllite.Utils.JsonRpc;
using Bryllite.Utils.NabiLog;
using Bryllite.Utils.Ntp;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b.Extension
{
    public class Web4bGameServer
    {
        public const string cyprus_getPoATokenSeed = "cyprus_getPoATokenSeed";
        public const string cyprus_getUserKeyExportToken = "cyprus_getUserKeyExportToken";

        /// <summary>
        /// cyprus api
        /// </summary>
        public readonly CyprusHelper Cyprus;

        /// <summary>
        /// game private key
        /// </summary>
        private readonly PrivateKey key;

        /// <summary>
        /// shop address
        /// </summary>
        private readonly Address shop;

        /// <summary>
        /// poa token seed
        /// should be updated from bridge service every token interval
        /// </summary>
        private string seed = Hex.ToString(SecureRandom.GetBytes(32));

        public Web4bGameServer(string url, string key, string shop)
        {
            this.key = key;
            this.shop = shop;

            // create cyprus api instance
            Cyprus = new CyprusHelper(url);
        }

        /// <summary>
        /// get user key by uid
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public string GetUserKey(string uid)
        {
            return key.CKD(uid);
        }

        /// <summary>
        /// get user address by uid
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public string GetUserAddress(string uid)
        {
            return key.CKD(uid).Address;
        }

        /// <summary>
        /// convert beryl to brc
        /// </summary>
        /// <param name="beryl"></param>
        /// <returns></returns>
        public decimal ToBrc(ulong beryl)
        {
            return Coin.ToCoin(beryl);
        }

        /// <summary>
        /// convert brc to beryl
        /// </summary>
        /// <param name="brc"></param>
        /// <returns></returns>
        public ulong ToBeryl(decimal brc)
        {
            return Coin.ToBeryl(brc);
        }

        /// <summary>
        /// update poa token seed from bridge service
        /// </summary>
        /// <returns></returns>
        public async Task<(string seed, string error)> UpdatePoATokenSeedAsync()
        {
            (string seed, string error) = await Cyprus.GetPoATokenSeedAsync(key);
            if (!string.IsNullOrEmpty(seed))
                this.seed = seed;

            return (seed, error);
        }

        /// <summary>
        /// get poa access token by uid, hash, iv
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="hash"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public (string token, string error) GetPoAToken(string uid, string hash, string iv)
        {
            try
            {
                // hash & iv
                byte[] hashbytes = Hex.ToByteArray(hash);
                byte[] ivbytes = Hex.ToByteArray(iv);

                // user address & salt
                byte[] address = key.CKD(uid).Address;
                string salt = hashbytes.Append(ivbytes).Append(address).ToHexString();

                // access token
                return (new BAuth(seed).GetAccessToken(salt), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get user key export token by uid
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<(string token, string error)> GetUserKeyExportTokenAsync(string uid, int id = 0)
        {
            return await Cyprus.GetUserKeyExportTokenAsync(key, uid, id);
        }

        /// <summary>
        /// get user balance
        /// </summary>
        /// <param name="address"></param>
        /// <param name="arg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(ulong? balance, string error)> GetBalanceAsync(string address, string arg = CyprusHelper.PENDING, int id = 0)
        {
            return await Cyprus.GetBalanceAsync(address, arg, id);
        }

        /// <summary>
        /// get user nonce
        /// </summary>
        /// <param name="address"></param>
        /// <param name="arg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(ulong? nonce, string error)> GetNonceAsync(string address, string arg = CyprusHelper.PENDING, int id = 0)
        {
            return await Cyprus.GetTransactionCountAsync(address, arg, id);
        }

        /// <summary>
        /// create tx
        /// </summary>
        /// <param name="chain"></param>
        /// <param name="signer"></param>
        /// <param name="to"></param>
        /// <param name="value"></param>
        /// <param name="gas"></param>
        /// <param name="nonce"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(Tx tx, string error)> CreateTxAsync(byte chain, string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            return await Cyprus.CreateTxAsync(chain, signer, to, value, gas, nonce, id);
        }

        /// <summary>
        /// send raw transfer
        /// </summary>
        /// <param name="rlp"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(string txid, string error)> SendRawTransferAsync(string rlp, int id = 0)
        {
            return await Cyprus.SendRawTransferAsync(rlp, id);
        }

        /// <summary>
        /// send raw withdraw
        /// </summary>
        /// <param name="rlp"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(string txid, string error)> SendRawWithdrawAsync(string rlp, int id = 0)
        {
            return await Cyprus.SendRawWithdrawAsync(rlp, id);
        }

        /// <summary>
        /// send transfer
        /// </summary>
        /// <param name="signer"></param>
        /// <param name="to"></param>
        /// <param name="value"></param>
        /// <param name="gas"></param>
        /// <param name="nonce"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(string txid, string error)> SendTransferAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            try
            {
                // create tx
                var res = await CreateTxAsync(Tx.Transfer, signer, to, value, gas, nonce, id);
                if (ReferenceEquals(res.tx, null))
                    return (null, res.error);

                string rlp = Hex.ToString(res.tx.Rlp);
                return await SendRawTransferAsync(rlp, id);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// send withdraw
        /// </summary>
        /// <param name="signer"></param>
        /// <param name="to"></param>
        /// <param name="value"></param>
        /// <param name="gas"></param>
        /// <param name="nonce"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(string txid, string error)> SendWithdrawAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            try
            {
                // create tx
                var res = await CreateTxAsync(Tx.Withdraw, signer, to, value, gas, nonce, id);
                if (ReferenceEquals(res.tx, null))
                    return (null, res.error);

                string rlp = Hex.ToString(res.tx.Rlp);
                return await SendRawWithdrawAsync(rlp, id);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get transaction receipt
        /// </summary>
        /// <param name="txid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(JObject receipt, string error)> GetTransactionReceiptAsync(string txid, int id = 0)
        {
            return await Cyprus.GetTransactionReceiptAsync(txid, id);
        }

        /// <summary>
        /// wait for transaction confirmed
        /// </summary>
        /// <param name="cancellation"></param>
        /// <param name="txid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(JObject receipt, string error)> WaitTransactionReceiptAsync(CancellationToken cancellation, string txid, int id = 0)
        {
            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    var res = await GetTransactionReceiptAsync(txid, id);
                    if (!ReferenceEquals(res.receipt, null))
                        return (res.receipt, null);

                    await Task.Delay(1000, cancellation);
                }

                return (null, "task canceled");
            }
            catch (TaskCanceledException tcex)
            {
                return (null, tcex.Message);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        public async Task<(JObject receipt, string error)> WaitTransactionReceiptAsync(string txid, int id = 0)
        {
            return await WaitTransactionReceiptAsync(CancellationToken.None, txid, id);
        }


        public async Task<(JObject receipt, string error)> SendWithdrawAndWaitReceiptAsync(CancellationToken cancellation, string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            // send withdraw transaction
            var res = await SendWithdrawAsync(signer, to, value, gas, nonce, id);
            if (ReferenceEquals(res, null))
                return (null, res.error);

            // wait for tx confirmed
            return await WaitTransactionReceiptAsync(cancellation, res.txid, id);
        }

        public async Task<(JObject receipt, string error)> SendWithdrawAndWaitReceiptAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            return await SendWithdrawAndWaitReceiptAsync(CancellationToken.None, signer, to, value, gas, nonce, id);
        }

    }
}
