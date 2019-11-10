using Bryllite.Utils.NabiLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b.Providers
{
    public class WebSocketProvider : IWeb4bProvider
    {
        // remote url
        private Uri remote;

        // websocket
        private ClientWebSocket connection;

        // receive buffer size
        public int? ReceiveBufferSize = null;

        // send buffer size
        public int? SendBufferSize = null;

        // keep alive time
        public int? KeepAliveTime = null;

        // message buffer size
        public static int BufferSize = 131072;

        // connect event handler
        public Action<bool> OnConnected;
        public Action<WebSocketCloseStatus?> OnDisconnected;
        public Action<string> OnReceived;

        // is conected?
        public bool Connected => connection?.State == WebSocketState.Open;

        // is connecting?
        public bool Connecting => connection?.State == WebSocketState.Connecting;

        public WebSocketProvider(Uri remote)
        {
            this.remote = remote;
        }

        public WebSocketProvider(string remote) : this(new Uri(remote))
        {
        }

        public void Dispose()
        {
            connection?.Dispose();
        }

        private async Task<bool> ConnectAsync(CancellationToken cancellation)
        {
            try
            {
                connection = new ClientWebSocket();

                // receive/send buffer
                if (ReceiveBufferSize != null && SendBufferSize != null)
                    connection.Options.SetBuffer(ReceiveBufferSize.Value, SendBufferSize.Value);

                // keep alive 
                if (KeepAliveTime != null)
                    connection.Options.KeepAliveInterval = TimeSpan.FromMilliseconds(KeepAliveTime.Value);

                // connect
                await connection.ConnectAsync(remote, cancellation);
                return Connected;
            }
            finally
            {
                OnConnected?.Invoke(Connected);
            }
        }

        public async Task SendAsync(string message, CancellationToken cancellation)
        {
            try
            {
                await connection.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, cancellation);
            }
            catch (Exception)
            {
                //Log.Warning("exception! ex=", ex);
            }
        }

        private async Task<string> ReceiveAsync(CancellationToken cancellation)
        {
            try
            {
                byte[] buffer = new byte[BufferSize];
                var result = await connection.ReceiveAsync(new ArraySegment<byte>(buffer), cancellation);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    OnDisconnected?.Invoke(WebSocketCloseStatus.NormalClosure);
                    return null;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                OnReceived?.Invoke(message);
                return message;
            }
            catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                OnDisconnected?.Invoke(WebSocketCloseStatus.NormalClosure);
            }
            catch (Exception)
            {
                //Log.Warning("exception! ex=", ex);
                OnDisconnected?.Invoke(WebSocketCloseStatus.InternalServerError);
            }

            return null;
        }

        public async Task<string> PostAsync(string body, CancellationToken cancellation)
        {
            // 연결 시도 중인 경우 exception
            if (Connecting) throw new WebSocketException(0, "connecting");

            // 연결되어 있지 않으면 연결한다
            if (!Connected)
            {
                bool connected = await ConnectAsync(cancellation);
                if (!connected)
                    throw new WebSocketException(1, "not connected");
            }

            try
            {
                // 메세지 전송
                await SendAsync(body, cancellation);

                // 응답 수신
                return await ReceiveAsync(cancellation);
            }
            catch (Exception ex)
            {
                connection?.Dispose();
                throw new WebSocketException("PostAsync() exception", ex);
            }
        }

        public async Task<string> PostAsync(string body)
        {
            return await PostAsync(body, CancellationToken.None);
        }

        public void Run(CancellationToken cancellation)
        {
            Task.Run(async () =>
            {
                await RunAsync(cancellation);
            });
        }

        public async Task RunAsync(CancellationToken cancellation)
        {
            // close status
            WebSocketCloseStatus? status = WebSocketCloseStatus.Empty;

            // receive buffer
            byte[] buffer = new byte[BufferSize];

            try
            {
                // connect
                await ConnectAsync(cancellation);

                WebSocketReceiveResult result = await connection.ReceiveAsync(new ArraySegment<byte>(buffer), cancellation);
                while (result.MessageType != WebSocketMessageType.Close)
                {
                    // received message
                    byte[] received = buffer.Take(result.Count).ToArray();

                    // invoke message callback
                    if (result.MessageType == WebSocketMessageType.Text)
                        OnReceived?.Invoke(Encoding.UTF8.GetString(received));
                    else Log.Warning("binary message not supported");

                    // receive again
                    result = await connection.ReceiveAsync(new ArraySegment<byte>(buffer), cancellation);
                }

                // close status
                status = result.CloseStatus;
            }
            catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                status = WebSocketCloseStatus.NormalClosure;
            }
            catch (Exception ex)
            {
                status = WebSocketCloseStatus.InternalServerError;
                Log.Warning("exception! ex.Message=", ex.Message);
            }
            finally
            {
                // disconnected event callback
                OnDisconnected?.Invoke(status);
                Dispose();
            }
        }
    }
}
