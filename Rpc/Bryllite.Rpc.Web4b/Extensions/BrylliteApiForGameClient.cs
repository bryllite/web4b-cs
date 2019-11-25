using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b.Extensions
{
    public class BrylliteApiForGameClient
    {
        // poa helper
        public PoAHelper PoA;

        // cyprus api
        public CyprusApi Cyprus;

        private string uid;
        private string address;

        public BrylliteApiForGameClient(string bridgeUrl, string poaUrl, PoAHelper.CallbackHandler callback)
        {
            // poa
            PoA = new PoAHelper(poaUrl, callback);

            // cyprus
            Cyprus = new CyprusApi(bridgeUrl);
        }

        // start poa service
        public void Start(string uid, string address)
        {
            this.uid = uid;
            this.address = address;

            PoA.Start(uid, address);
        }

        // stop poa service
        public void Stop()
        {
            PoA.Stop();
        }

        // get balance
        public async Task<ulong> GetBalanceAsync()
        {
            return await Cyprus.GetBalanceAsync(address, CyprusApi.LATEST) ?? 0;
        }

        // get tx history
        public async Task<JArray> GetTransactionHistoryAsync(bool tx)
        {
            return await Cyprus.GetTransactionsByAddressAsync(address, tx);
        }

    }
}
