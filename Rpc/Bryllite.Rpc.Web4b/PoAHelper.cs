using Bryllite.Cryptography.Signers;
using Bryllite.Extensions;
using Bryllite.Rpc.Web4b.Providers;
using Bryllite.Utils.JsonRpc;
using Bryllite.Utils.NabiLog;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b
{
    public class PoAHelper : IDisposable
    {
        public const string POA_REQUEST = "poa_request_subscription";
        public const string POA_RESPONSE = "poa_response";

        // accessToken callback handler
        public delegate Task<string> CallbackHandler(string hash, string iv);
        private CallbackHandler callback;

        // websocket connection
        private WebSocketProvider connection;

        // user id
        private string uid;

        // user address
        private string address;
        public string Address => address;

        // is connected?
        public bool Connected => connection.Connected;

        public PoAHelper(string remote, CallbackHandler callback)
        {
            this.callback = callback;

            // websocket connection
            connection = new WebSocketProvider(remote)
            {
                OnConnected = OnConnected,
                OnDisconnected = OnDisconnected,
                OnReceived = OnMessage
            };
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
            Task task = null;
            try
            {
                var request = JsonRpc.Parse(message);
                switch (request.Method)
                {
                    case POA_REQUEST: task = OnMessagePoARequest(request); return;
                    default: break;
                }
            }
            catch (Exception ex)
            {
                Log.Warning("exception! ex.Message=", ex.Message, ", message=", message);
            }
        }

        private async Task OnMessagePoARequest(JsonRpc request)
        {
            string hash = request.Params<string>(0);
            string iv = request.Params<string>(1);

            // request for accessToken
            string accessToken = await callback?.Invoke(hash, iv);
            if (!string.IsNullOrEmpty(accessToken))
                await connection.SendAsync(new JsonRpc.Notification(POA_RESPONSE, uid, address, accessToken).ToString(), CancellationToken.None);
        }

    }
}
