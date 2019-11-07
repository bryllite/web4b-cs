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
        // accessToken callback handler
        public delegate string CallbackHandler(string hash, string iv);
        private CallbackHandler callback;

        private Uri remote;

        private WebSocketProvider connection;

        // user address
        private Address address;

        // is connected?
        public bool Connected => connection.Connected;

        public PoAHelper(Uri remote, Address address, CallbackHandler callback)
        {
            this.remote = remote;
            this.address = address;
            this.callback = callback;

            // web socket connection
            connection = new WebSocketProvider(remote)
            {
                OnConnected = OnConnected,
                OnDisconnected = OnDisconnected,
                OnReceived = OnMessage
            };
        }

        public PoAHelper(string remote, Address address, CallbackHandler callback) : this(new Uri(remote), address, callback)
        {
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public void Start(CancellationToken cancellation)
        {
            connection.Run(cancellation);
        }

        public void Stop(CancellationToken cancellation)
        {
            connection.Dispose();
        }

        // 연결 실패인 경우 retry 여부
        public void OnConnected(bool connected)
        {
            Log.Debug(connected ? "connected!" : "not connected!");
        }

        // 연결이 끊어진 경우 retry 여부
        public void OnDisconnected(WebSocketCloseStatus? status)
        {
            Log.Debug("connection lost!");
        }

        // 메세지 수신
        public void OnMessage(string message)
        {
            Task task = null;
            try
            {
                var request = JsonRpc.Parse(message);
                switch (request.Method)
                {
                    case "poa.request": task = OnMessagePoARequest(request); return;
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
            string accessToken = callback?.Invoke(hash, iv);
            if (!accessToken.IsNullOrEmpty())
                await connection.SendAsync(new JsonRpc.Notification("poa.response", address.Hex, accessToken).ToString(), CancellationToken.None);
        }
    }
}
