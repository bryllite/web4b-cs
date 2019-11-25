using Bryllite.Core;
using Bryllite.Cryptography.Aes;
using Bryllite.Cryptography.Signers;
using Bryllite.Extensions;
using Bryllite.Rpc.Web4b.Providers;
using Bryllite.Utils.JsonRpc;
using Bryllite.Utils.NabiLog;
using Bryllite.Utils.Ntp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b
{
    public class CyprusApi : ApiBase
    {
        public CyprusApi(IWeb4bProvider web4b) : base(web4b)
        {
        }

        public CyprusApi(Uri remote) : base(remote)
        {
        }

        public CyprusApi(string remote) : base(remote)
        {
        }

        public async Task<string> GetCoinbaseAsync(int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("cyprus_coinbase", id));
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

        public async Task<long?> GetBlockNumberAsync(int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("cyprus_blockNumber", id));
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

        public async Task<string> GetAddressByUid(string uid, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("cyprus_getAddressByUid", id, uid));
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

        public async Task<ulong?> GetBalanceAsync(string address, string number, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("cyprus_getBalance", id, address, number));
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

        public async Task<ulong?> GetBalanceByUidAsync(string uid, string number, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("cyprus_getBalanceByUid", id, uid, number));
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
                var response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionCount", id, address, number));
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

        public async Task<ulong?> GetTransactionCountByUidAsync(string uid, string number, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionCountByUid", id, uid, number));
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
                var response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionByHash", id, txid));
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
                var response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionByBlockNumberAndIndex", id, Hex.ToString(number, true), Hex.ToString(index, true)));
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
                var response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionByBlockHashAndIndex", id, hash, Hex.ToString(index, true)));
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

        public async Task<Tx> CreateTx(byte opcode, string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null)
        {
            try
            {
                // nonce?
                if (null == nonce)
                    nonce = await GetTransactionCountAsync(new PrivateKey(signer).Address, PENDING);

                // make tx
                return new Tx()
                {
                    Chain = opcode,
                    To = to,
                    Value = value,
                    Gas = gas,
                    Nonce = nonce.Value
                }.Sign(signer);
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<string> TransferAsync(Tx tx, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("cyprus_sendTransferTransaction", id, Hex.ToString(tx.Rlp)));
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

        public async Task<string> WithdrawAsync(Tx tx, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("cyprus_sendWithdrawTransaction", id, Hex.ToString(tx.Rlp)));
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


        public async Task<string> TransferAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            var tx = await CreateTx(Tx.Transfer, signer, to, value, gas, nonce);
            return !ReferenceEquals(tx, null) ? await TransferAsync(tx, id) : null;
        }

        public async Task<JObject> TransferAndWaitReceiptAsync(CancellationToken cancellation, string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            var txid = await TransferAsync(signer, to, value, gas, nonce, id);
            if (string.IsNullOrEmpty(txid))
                return null;

            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    await Task.Delay(10, cancellation);

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

        public async Task<JObject> TransferAndWaitReceiptAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            return await TransferAndWaitReceiptAsync(CancellationToken.None, signer, to, value, gas, nonce, id);
        }

        public async Task<string> WithdrawAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            var tx = await CreateTx(Tx.Withdraw, signer, to, value, gas, nonce);
            return !ReferenceEquals(tx, null) ? await WithdrawAsync(tx, id) : null;
        }

        public async Task<JObject> WithdrawAndWaitReceiptAsync(CancellationToken cancellation, string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            var txid = await WithdrawAsync(signer, to, value, gas, nonce, id);
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

        public async Task<JObject> WithdrawAndWaitReceiptAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            return await WithdrawAndWaitReceiptAsync(CancellationToken.None, signer, to, value, gas, nonce, id);
        }

        public async Task<JArray> GetPendingTransactionsAsync(int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("cyprus_pendingTransactions", id));
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
                var response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionReceipt", id, txid));
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

        public async Task<JObject> GetBlockByHashAsync(string hash, bool tx, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("cyprus_getBlockByHash", id, hash, Hex.ToString(tx, true)));
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
                var response = await PostAsync(new JsonRpc.Request("cyprus_getBlockByNumber", id, Hex.ToString(number, true), Hex.ToString(tx, true)));
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
                var response = await PostAsync(new JsonRpc.Request("cyprus_getBlockByNumber", id, Hex.ToString(number, true), Hex.ToString(0x02, true)));
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
                var response = await PostAsync(new JsonRpc.Request("cyprus_getBlockTransactionCountByHash", id, hash));
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
                var response = await PostAsync(new JsonRpc.Request("cyprus_getBlockTransactionCountByNumber", id, Hex.ToString(number, true)));
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
                var response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionsByAddress", id, address, tx));
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

        public async Task<JArray> GetTransactionsByUidAsync(string uid, bool tx = false, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("cyprus_getTransactionsByUid", id, uid, tx));
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

        public async Task<string> ExportKeyTokenAsync(string uid, string signature, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("cyprus_getUserKeyExportToken", id, uid, signature));
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

        public async Task<string> ExportKeyAsync(string token, int id = 0)
        {
            PrivateKey nonce = PrivateKey.CreateKey();

            try
            {
                var response = await PostAsync(new JsonRpc.Request("cyprus_getUserKey", id, token, Hex.ToString(nonce.PublicKey.CompressedKey)));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                    {
                        string encrypted = response.Result<string>(0);
                        string openKey = response.Result<string>(1);
                        string passKey = nonce.CreateEcdhKey(openKey);

                        // decrypt user key
                        if (Aes256.TryDecrypt(Hex.ToByteArray(passKey), Hex.ToByteArray(encrypted), out var plain))
                            return Hex.ToString(plain);

                        SetLastError("aes-256 decrypt failed");
                    }
                    else
                    {
                        SetLastError(response.ErrorMessage);
                    }
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
