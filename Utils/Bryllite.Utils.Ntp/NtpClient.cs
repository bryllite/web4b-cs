using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Bryllite.Utils.Ntp
{
    public class NtpClient : IDisposable
    {
        public const int NTP_PORT = 123;

        public const int DEFAULT_TIMEOUT = 500;

        private readonly Socket socket;

        public TimeSpan Timeout
        {
            get { return TimeSpan.FromMilliseconds(socket.ReceiveTimeout); }
            set
            {
                if (value < TimeSpan.FromMilliseconds(1))
                    throw new ArgumentOutOfRangeException();
                socket.ReceiveTimeout = Convert.ToInt32(value.TotalMilliseconds);
            }
        }

        public NtpClient(IPEndPoint endpoint)
        {
            socket = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.ReceiveTimeout = DEFAULT_TIMEOUT;
                socket.Connect(endpoint);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }

        public NtpClient(IPAddress address, int port = NTP_PORT) : this(new IPEndPoint(address, port))
        {
        }

        public void Dispose() { socket.Dispose(); }

        public TimeSpan GetOffset() { return Query().Offset; }

        public NtpPacket Query(NtpPacket request)
        {
            socket.Send(request.Bytes);
            var response = new byte[160];
            int received = socket.Receive(response);
            var truncated = new byte[received];
            Array.Copy(response, truncated, received);
            return new NtpPacket(truncated) { DestinationTimestamp = DateTime.UtcNow };
        }

        public NtpPacket Query() { return Query(new NtpPacket()); }
    }
}
