using Bryllite.Utils.NabiLog;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Bryllite.App.Sample.TcpGameClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Filter = LogLevel.All;

            new GameClientApplication(args).Run();
        }
    }
}
