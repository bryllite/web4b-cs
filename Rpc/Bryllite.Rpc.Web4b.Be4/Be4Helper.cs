using Bryllite.Core;
using Bryllite.Cryptography.Signers;
using Bryllite.Rpc.Web4b.Providers;
using Bryllite.Utils.JsonRpc;
using Bryllite.Utils.Ntp;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b.Be4
{
    public class Be4Helper : Web4b
    {
        public const string be4_coinbase = "be4_coinbase";
        public const string be4_blockNumber = "be4_blockNumber";
        public const string be4_getBalance = "be4_getBalance";
        public const string be4_getTransactionCount = "be4_getTransactionCount";
        public const string be4_getBlockTransactionCountByHash = "be4_getBlockTransactionCountByHash";
        public const string be4_getBlockTransactionCountByNumber = "be4_getBlockTransactionCountByNumber";
        public const string be4_sendRawTransaction = "be4_sendRawTransaction";
        public const string be4_getBlockByHash = "be4_getBlockByHash";
        public const string be4_getBlockByNumber = "be4_getBlockByNumber";
        public const string be4_getTransactionByHash = "be4_GetTransactionByHash";
        public const string be4_getTransactionByBlockHashAndIndex = "be4_getTransactionByBlockHashAndIndex";
        public const string be4_getTransactionByBlockNumberAndIndex = "be4_getTransactionByBlockNumberAndIndex";
        public const string be4_getTransactionReceipt = "be4_getTransactionReceipt";
        public const string be4_pendingTransactions = "be4_pendingTransactions";
        public const string be4_getTransactionsByAddress = "be4_getTransactionsByAddress";

        public const string EARLIEST = "earliest";
        public const string LATEST = "latest";
        public const string PENDING = "pending";

        public Be4Helper(IWeb4bProvider provider) : base(provider)
        {
        }

        public Be4Helper(Uri remote) : base(remote)
        {
        }

        public Be4Helper(string remote) : base(remote)
        {
        }

        /// <summary>
        /// get coinbase address
        /// method: be4_coinbase
        /// params: none
        /// result: coinbase address 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(string coinbase, string error)> GetCoinbaseAsync(int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_coinbase, id));
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
        /// method: be4_blockNumber
        /// params: none
        /// result: latest block number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(long number, string error)> GetBlockNumberAsync(int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_blockNumber, id));
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
        /// method: be4_getBalance
        /// params: (address, args: block number in hex or one of these "earliest", "latest", "pending" )
        /// result: balance of address in beryl
        /// </summary>
        /// <param name="address"></param>
        /// <param name="arg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(ulong? balance, string error)> GetBalanceAsync(string address, string arg = LATEST, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_getBalance, id, address, arg));
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
        /// method: be4_getTransactionCount
        /// params: (address, args: block number in hex or one of these "earliest", "latest", "pending" )
        /// result: total transaction count of address a.k.a nonce
        /// </summary>
        /// <param name="address"></param>
        /// <param name="arg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(ulong? nonce, string error)> GetTransactionCountAsync(string address, string arg = LATEST, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_getTransactionCount, id, address, arg));
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
        /// method: be4_getBlockTransactionCountByHash
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
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_getBlockTransactionCountByHash, id, hash));
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
        /// method: be4_getBlockTransactionCountByNumber
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
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_getBlockTransactionCountByNumber, id, Hex.ToString(number, true)));
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
        /// send transaction by tx.rlp
        /// method: be4_sendRawTransaction
        /// params: hex string of tx.rlp
        /// result: txid
        /// </summary>
        /// <param name="rlp"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(string txid, string error)> SendRawTransactionAsync(string rlp, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_sendRawTransaction, id, rlp));
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
        /// method: be4_getBlockByHash
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
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_getBlockByHash, id, hash, Hex.ToString(arg, true)));
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
        /// method: be4_getBlockByNumber
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
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_getBlockByNumber, id, Hex.ToString(number, true), Hex.ToString(arg, true)));
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
        /// method: be4_getBlockByHash
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
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_getBlockByHash, id, hash, Hex.ToString(0x02, true)));
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
        /// method: be4_getBlockByNumber
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
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_getBlockByNumber, id, Hex.ToString(number, true), Hex.ToString(0x02, true)));
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
        /// method: be4_getTransactionByHash
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
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_getTransactionByHash, id, hash));
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
        /// method: be4_getTransactionByBlockHashAndIndex
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
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_getTransactionByBlockHashAndIndex, id, hash, Hex.ToString(index, true)));
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
        /// method: be4_getTransactionByBlockNumberAndIndex
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
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_getTransactionByBlockNumberAndIndex, id, Hex.ToString(number, true), Hex.ToString(index, true)));
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
        /// method: be4_getTransactionReceipt
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
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_getTransactionReceipt, id, txid));
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
        /// get pending transactions
        /// method: be4_pendingTransactions
        /// params: none
        /// result: array of pending transaction(JObject)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(JArray txs, string error)> GetPendingTransactionsAsync(int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_pendingTransactions, id));
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
        public async Task<(Tx tx, string error)> CreateTxAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
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

        // send tx
        public async Task<(string txid, string error)> SendTransactionAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            // create tx
            (Tx tx, string error) = await CreateTxAsync(signer, to, value, gas, nonce, id);
            if (ReferenceEquals(tx, null))
                return (null, error);

            // send tx
            return await SendRawTransactionAsync(Hex.ToString(tx.Rlp), id);
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
        public async Task<(JObject receipt, string error)> SendTransactionAndWaitReceiptAsync(CancellationToken cancellation, string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            // send tx
            (string txid, string error) = await SendTransactionAsync(signer, to, value, gas, nonce, id);
            if (ReferenceEquals(txid, null))
                return (null, error);

            // wait for tx receipt
            return await WaitTransactionReceiptAsync(cancellation, txid, id);
        }

        public async Task<(JObject receipt, string error)> SendTransactionAndWaitReceiptAsync(string signer, string to, ulong value, ulong gas = 0, ulong? nonce = null, int id = 0)
        {
            return await SendTransactionAndWaitReceiptAsync(CancellationToken.None, signer, to, value, gas, nonce, id);
        }


        // get transaction history of address
        public async Task<(JArray txs, string error)> GetTransactionsByAddressAsync(string address, long start, bool desc, int max, int id = 0)
        {
            try
            {
                (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(be4_getTransactionsByAddress, id, address, Hex.ToString(start, true), desc ? "desc" : "asc", Hex.ToString(max, true)));
                if (ReferenceEquals(response, null))
                    return (null, error);

                return (response.GetResult(), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
    }
}
