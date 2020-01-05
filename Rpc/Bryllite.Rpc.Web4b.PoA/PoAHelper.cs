using Bryllite.Rpc.Web4b.Cyprus;
using Bryllite.Rpc.Web4b.Providers;
using Bryllite.Utils.JsonRpc;
using Bryllite.Utils.NabiLog;
using Newtonsoft.Json.Linq;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b.PoA
{
    public class PoAHelper : IDisposable
    {
        public const string POA_REQUEST = "poa_request_subscription";
        public const string POA_RESPONSE = "poa_response";
        public const string POA_RESULT = "poa_result";

        // accessToken callback handler
        public delegate Task<string> CallbackHandler(string hash, string iv);
        private CallbackHandler callback;

        // websocket connection
        private WebSocketProvider connection;

        // cyprus helper
        public CyprusHelper Cyprus;

        // user id
        private string uid;

        // user address
        private string address;
        public string Address => address;

        // is connected?
        public bool Connected => connection.Connected;

        public PoAHelper(string endpointPoA, CallbackHandler callback, string endpointCyprus = null)
        {
            this.callback = callback;

            // websocket connection
            connection = new WebSocketProvider(endpointPoA)
            {
                OnConnected = OnConnected,
                OnDisconnected = OnDisconnected,
                OnReceived = OnMessage
            };

            // cyprus helper
            if (!string.IsNullOrEmpty(endpointCyprus))
                Cyprus = new CyprusHelper(endpointCyprus);
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public void Start(string uid, string address, CancellationToken cancellation)
        {
            this.uid = uid;
            this.address = address;

            connection.Run(cancellation);
        }

        public void Start(string uid, string address)
        {
            Start(uid, address, CancellationToken.None);
        }

        public void Stop()
        {
            Dispose();
        }

        // 연결 실패인 경우 retry 여부
        private void OnConnected(bool connected)
        {
            Log.Debug(connected ? "connected!" : "not connected!");
        }

        // 연결이 끊어진 경우 retry 여부
        private void OnDisconnected(WebSocketCloseStatus? status)
        {
            Log.Debug("connection lost!");
        }

        // 메세지 수신
        private void OnMessage(string message)
        {
            try
            {
                var request = JsonRpc.Parse(message);
                switch (request.Method)
                {
                    case POA_REQUEST: OnMessagePoARequest(request); return;
                    case POA_RESULT: OnMessagePoAResult(request); return;
                    default: break;
                }
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", message=", message);
            }
        }

        private void OnMessagePoARequest(JsonRpc request)
        {
            Task.Run(async () =>
            {
                string hash = request.Params<string>(0);
                string iv = request.Params<string>(1);

                // request for accessToken
                string accessToken = await callback?.Invoke(hash, iv);
                if (!string.IsNullOrEmpty(accessToken))
                    await connection.SendAsync(new JsonRpc.Notification(POA_RESPONSE, uid, address, accessToken).ToString(), CancellationToken.None);
            });
        }

        private void OnMessagePoAResult(JsonRpc request)
        {
            string error = request.Params<string>(0);
            if (!string.IsNullOrEmpty(error))
                Log.Warning("PoA.ErrorMessage=", error);
        }


        public async Task<(ulong? balance, string error)> GetBalanceAsync(string arg = CyprusHelper.PENDING, int id = 0)
        {
            if (ReferenceEquals(Cyprus, null))
                return (null, "cyprus provider not exists");

            return await Cyprus.GetBalanceAsync(address, arg, id);
        }

        public async Task<(JArray txs, string error)> GetTransactionHistoryAsync(long start, bool desc, int max, int id = 0)
        {
            if (ReferenceEquals(Cyprus, null))
                return (null, "cyprus provider not exists");

            return await Cyprus.GetTransactionsByAddressAsync(address, start, desc, max, id);
        }

    }
}
