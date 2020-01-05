using Bryllite.Core;
using Bryllite.Cryptography.Aes;
using Bryllite.Cryptography.Hash.Extensions;
using Bryllite.Cryptography.Signers;
using Bryllite.Rpc.Web4b.Providers;
using Bryllite.Utils.JsonRpc;
using Bryllite.Utils.Ntp;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b.Cyprus
{
    public class CyprusHelper : Web4b
    {
        public const string cyprus_coinbase = "cyprus_coinbase";
        public const string cyprus_blockNumber = "cyprus_blockNumber";
        public const string cyprus_getBalance = "cyprus_getBalance";
        public const string cyprus_getBalanceByUid = "cyprus_getBalanceByUid";
        public const string cyprus_getTransactionCount = "cyprus_getTransactionCount";
        public const string cyprus_getTransactionCountByUid = "cyprus_getTransactionCountByUid";
        public const string cyprus_getBlockTransactionCountByHash = "cyprus_getBlockTransactionCountByHash";
        public const string cyprus_getBlockTransactionCountByNumber = "cyprus_getBlockTransactionCountByNumber";
        public const string cyprus_sendRawTransaction = "cyprus_sendRawTransaction";
        public const string cyprus_sendRawTransfer = "cyprus_sendRawTransfer";
        public const string cyprus_sendRawWithdraw = "cyprus_sendRawWithdraw";
        public const string cyprus_getBlockByHash = "cyprus_getBlockByHash";
        public const string cyprus_getBlockByNumber = "cyprus_getBlockByNumber";
        public const string cyprus_getTransactionByHash = "cyprus_GetTransactionByHash";
        public const string cyprus_getTransactionByBlockHashAndIndex = "cyprus_getTransactionByBlockHashAndIndex";
        public const string cyprus_getTransactionByBlockNumberAndIndex = "cyprus_getTransactionByBlockNumberAndIndex";
        public const string cyprus_getTransactionReceipt = "cyprus_getTransactionReceipt";
        public const string cyprus_getTransactionsByAddress = "cyprus_getTransactionsByAddress";
        public const string cyprus_getTransactionsByUid = "cyprus_getTransactionsByUid";

        public const string cyprus_getPoATokenSeed = "cyprus_getPoATokenSeed";
        public const string cyprus_getUserKeyExportToken = "cyprus_getUserKeyExportToken";
        public const string cyprus_getUserKey = "cyprus_getUserKey";
        public const string cyprus_getAddressByUid = "cyprus_getAddressByUid";
        public const string cyprus_getUidByAddress = "cyprus_getUidByAddress";

        public const string EARLIEST = "earliest";
        public const string LATEST = "latest";
        public const string PENDING = "pending";

        public CyprusHelper(IWeb4bProvider provider) : base(provider)
        {
        }

        public CyprusHelper(Uri remote) : base(remote)
        {
        }

        public CyprusHelper(string remote) : base(remote)
        {
        }

        /// <summary>
        /// get coinbase address
        /// method: cyprus_coinbase
        /// params: none
        /// result: coinbase address 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(string coinbase, string error)> GetCoinbaseAsync(int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_coinbase, id));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<string>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get latest block number
        /// method: cyprus_blockNumber
        /// params: none
        /// result: latest block number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(long number, string error)> GetBlockNumberAsync(int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_blockNumber, id));
                if (ReferenceEquals(response, null))
                    return (-1, error);

                return (Hex.ToNumber<long>(response.Result<string>(0)), null);
            }
            catch (Exception ex)
            {
                return (-1, ex.Message);
            }
        }

        /// <summary>
        /// get balance of address
        /// method: cyprus_getBalance
        /// params: (address, args: block number in hex or one of these "earliest", "latest", "pending" )
        /// result: balance of address in beryl
        /// </summary>
        /// <param name="address"></param>
        /// <param name="arg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(ulong? balance, string error)> GetBalanceAsync(string address, string arg = PENDING, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getBalance, id, address, arg));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (Hex.ToNumber<ulong>(response.Result<string>(0)), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get balance of uid
        /// method: cyprus_getBalanceByUid
        /// params: (uid, args: block number in hex or one of these "earliest", "latest", "pending" )
        /// result: balance of uid in beryl
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="arg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(ulong? balance, string error)> GetBalanceByUidAsync(string uid, string arg, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getBalanceByUid, id, uid, arg));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (Hex.ToNumber<ulong>(response.Result<string>(0)), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get transaction count of address
        /// method: cyprus_getTransactionCount
        /// params: (address, args: block number in hex or one of these "earliest", "latest", "pending" )
        /// result: total transaction count of address a.k.a nonce
        /// </summary>
        /// <param name="address"></param>
        /// <param name="arg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(ulong? nonce, string error)> GetTransactionCountAsync(string address, string arg, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getTransactionCount, id, address, arg));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (Hex.ToNumber<ulong>(response.Result<string>(0)), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get transaction count of uid
        /// method: cyprus_getTransactionCountByUid
        /// params: (uid, args: block number in hex or one of these "earliest", "latest", "pending" )
        /// result: total transaction count of uid a.k.a nonce
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="arg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(ulong? nonce, string error)> GetTransactionCountByUidAsync(string uid, string arg, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getTransactionCountByUid, id, uid, arg));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (Hex.ToNumber<ulong>(response.Result<string>(0)), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
        /// <summary>
        /// get transaction count of block by block hash
        /// method: cyprus_getBlockTransactionCountByHash
        /// params: block hash
        /// result: transaction count in block
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(int count, string error)> GetBlockTransactionCountByHashAsync(string hash, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getBlockTransactionCountByHash, id, hash));
                if (ReferenceEquals(response, null))
                    return (-1, error);

                return (Hex.ToNumber<int>(response.Result<string>(0)), null);
            }
            catch (Exception ex)
            {
                return (-1, ex.Message);
            }
        }

        /// <summary>
        /// get transaction count of block by block number
        /// method: cyprus_getBlockTransactionCountByNumber
        /// params: block number
        /// result: transaction count in block
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(int count, string error)> GetBlockTransactionCountByNumberAsync(long number, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getBlockTransactionCountByNumber, id, Hex.ToString(number, true)));
                if (ReferenceEquals(response, null))
                    return (-1, error);

                return (Hex.ToNumber<int>(response.Result<string>(0)), null);
            }
            catch (Exception ex)
            {
                return (-1, ex.Message);
            }
        }

        /// <summary>
        /// send raw transaction
        /// method: cyprus_sendRawTransaction
        /// params: tx rlp
        /// result: txid
        /// </summary>
        /// <param name="rlp"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(string txid, string error)> SendRawTransactionAsync(string rlp, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_sendRawTransaction, id, rlp));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<string>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// send raw transfer
        /// method: cyprus_sendRawTransfer
        /// params: tx rlp
        /// result: txid
        /// </summary>
        /// <param name="rlp"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(string txid, string error)> SendRawTransferAsync(string rlp, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_sendRawTransfer, id, rlp));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<string>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// send raw withdraw
        /// method: cyprus_sendRawWithdraw
        /// params: tx rlp
        /// result: txid
        /// </summary>
        /// <param name="rlp"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(string txid, string error)> SendRawWithdrawAsync(string rlp, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_sendRawWithdraw, id, rlp));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<string>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get block object by block hash
        /// method: cyprus_getBlockByHash
        /// params: block hash, tx object?
        /// result: JObject of block
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="arg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(JObject block, string error)> GetBlockByHashAsync(string hash, bool arg, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getBlockByHash, id, hash, Hex.ToString(arg, true)));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<JObject>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get block object by block number
        /// method: cyprus_getBlockByNumber
        /// params: block hash, tx object?
        /// result: JObject of block
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="arg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(JObject block, string error)> GetBlockByNumberAsync(long number, bool arg, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getBlockByNumber, id, Hex.ToString(number, true), Hex.ToString(arg, true)));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<JObject>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get block rlp by block hash
        /// method: cyprus_getBlockByHash
        /// params: block hash
        /// result: hex string of block rlp
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="arg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(string rlp, string error)> GetBlockRlpByHashAsync(string hash, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getBlockByHash, id, hash, Hex.ToString(0x02, true)));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<string>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get block rlp by block number
        /// method: cyprus_getBlockByNumber
        /// params: block number
        /// result: hex string of block rlp
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="arg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(string rlp, string error)> GetBlockRlpByNumberAsync(long number, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getBlockByNumber, id, Hex.ToString(number, true), Hex.ToString(0x02, true)));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<string>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get transaction object by tx hash
        /// method: cyprus_getTransactionByHash
        /// params: txid
        /// result: JObject of tx
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(JObject tx, string error)> GetTransactionByHashAsync(string hash, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getTransactionByHash, id, hash));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<JObject>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get transaction object by block hash and index
        /// method: cyprus_getTransactionByBlockHashAndIndex
        /// params: block hash, tx index
        /// result: transaction object
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="index"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(JObject tx, string error)> GetTransactionByBlockHashAndIndexAsync(string hash, int index, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getTransactionByBlockHashAndIndex, id, hash, Hex.ToString(index, true)));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<JObject>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get transaction object by block number and index
        /// method: cyprus_getTransactionByBlockNumberAndIndex
        /// params: block number, tx index
        /// result: transaction object
        /// </summary>
        /// <param name="number"></param>
        /// <param name="index"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(JObject tx, string error)> GetTransactionByBlockNumberAndIndexAsync(long number, int index, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getTransactionByBlockNumberAndIndex, id, Hex.ToString(number, true), Hex.ToString(index, true)));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<JObject>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get transaction receipt object
        /// method: cyprus_getTransactionReceipt
        /// params: tx hash (txid)
        /// result: receipt transaction object
        /// </summary>
        /// <param name="txid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(JObject receipt, string error)> GetTransactionReceiptAsync(string txid, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getTransactionReceipt, id, txid));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<JObject>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get transaction history of address
        /// method: cyprus_getTransactionsByAddress
        /// params: address, start block number, sort, max tx count
        /// result: JArray<Tx>
        /// </summary>
        /// <param name="address"></param>
        /// <param name="start"></param>
        /// <param name="desc"></param>
        /// <param name="max"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(JArray txs, string error)> GetTransactionsByAddressAsync(string address, long start, bool desc, int max, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getTransactionsByAddress, id, address, Hex.ToString(start, true), desc ? "desc" : "asc", Hex.ToString(max, true)));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.GetResult(), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get transaction history of uid
        /// method: cyprus_getTransactionsByUid
        /// params: uid, start block number, sort, max tx count
        /// result: JArray<Tx>
        /// </summary>
        /// <param name="address"></param>
        /// <param name="start"></param>
        /// <param name="desc"></param>
        /// <param name="max"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(JArray txs, string error)> GetTransactionsByUidAsync(string uid, long start, bool desc, int max, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getTransactionsByUid, id, uid, Hex.ToString(start, true), desc ? "desc" : "asc", Hex.ToString(max, true)));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.GetResult(), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        // create tx
        public async Task<(Tx tx, string error)> CreateTxAsync(byte chain, string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            try
            {
                // signer key
                PrivateKey key = signer;

                // nonce?
                if (null == nonce)
                {
                    string error = null;
                    (nonce, error) = await GetTransactionCountAsync(key.Address, PENDING, id);
                    if (null == nonce)
                        return (null, error);
                }

                // make tx
                return (new Tx()
                {
                    Chain = chain,
                    To = to,
                    Value = value,
                    Gas = gas,
                    Nonce = nonce.Value
                }.Sign(key), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        // transfer
        public async Task<(string txid, string error)> SendTransferAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            // create tx
            (Tx tx, string error) = await CreateTxAsync(Tx.Transfer, signer, to, value, gas, nonce, id);
            if (ReferenceEquals(tx, null))
                return (null, error);

            // send tx
            return await SendRawTransferAsync(Hex.ToString(tx.Rlp), id);
        }

        // withdraw
        public async Task<(string txid, string error)> SendWithdrawAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            // create tx
            (Tx tx, string error) = await CreateTxAsync(Tx.Withdraw, signer, to, value, gas, nonce, id);
            if (ReferenceEquals(tx, null))
                return (null, error);

            // send tx
            return await SendRawWithdrawAsync(Hex.ToString(tx.Rlp), id);
        }

        // wait tx confirmed
        public async Task<(JObject receipt, string error)> WaitTransactionReceiptAsync(CancellationToken cancellation, string txid, int id = 0)
        {
            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    await NetTime.Sleep(1000, cancellation);

                    var response = await GetTransactionReceiptAsync(txid, id);
                    if (!ReferenceEquals(response.receipt, null))
                        return (response.receipt, null);
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


        // send tx and wait tx confirmed
        public async Task<(JObject receipt, string error)> SendWithdrawAndWaitReceiptAsync(CancellationToken cancellation, string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            // send tx
            (string txid, string error) = await SendWithdrawAsync(signer, to, value, gas, nonce, id);
            if (ReferenceEquals(txid, null))
                return (null, error);

            // wait for tx receipt
            return await WaitTransactionReceiptAsync(cancellation, txid, id);
        }

        public async Task<(JObject receipt, string error)> SendWithdrawAndWaitReceiptAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            return await SendWithdrawAndWaitReceiptAsync(CancellationToken.None, signer, to, value, gas, nonce, id);
        }


        public async Task<(string seed, string error)> GetPoATokenSeedAsync(string signer, int id = 0)
        {
            try
            {
                PrivateKey key = signer;
                string timestamp = Hex.ToString(NetTime.UnixTime, true);
                byte[] messageHash = Hex.ToByteArray(timestamp).Hash256();
                string signature = key.Sign(messageHash);

                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getPoATokenSeed, 0, timestamp, signature));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<string>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }


        public async Task<(string token, string error)> GetUserKeyExportTokenAsync(string signer, string uid, int id = 0)
        {
            try
            {
                PrivateKey key = signer;
                string signature = key.Sign(Encoding.UTF8.GetBytes(uid).Hash256());

                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getUserKeyExportToken, 0, uid, signature));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<string>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }


        public async Task<(string key, string error)> GetUserKeyAsync(string token, int id = 0)
        {
            try
            {
                PrivateKey nonce = PrivateKey.CreateKey();

                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getUserKey, id, token, Hex.ToString(nonce.PublicKey.CompressedKey)));
                if (ReferenceEquals(response, null))
                    return (null, error);

                string encrypted = response.Result<string>(0);
                string openKey = response.Result<string>(1);
                string passKey = nonce.CreateEcdhKey(openKey);

                // decrypt user key
                return Aes256.TryDecrypt(Hex.ToByteArray(passKey), Hex.ToByteArray(encrypted), out var plain) ? (Hex.ToString(plain), (string)null) : (null, "can't decrypt key");
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }


        public async Task<(string address, string error)> GetAddressByUidAsync(string uid, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getAddressByUid, id, uid));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<string>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }


        public async Task<(string uid, string error)> GetUidByAddress(string address, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(cyprus_getUidByAddress, id, address));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.Result<string>(0), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

    }
}
