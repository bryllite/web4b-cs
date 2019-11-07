using System;
using System.Net;
using Bryllite.Utils.NabiLog;

namespace Bryllite.Utils.Ntp
{
    public class NetTime
    {
        public static readonly DateTime EpochTime = new DateTime(1970, 1, 1);
        public static readonly DateTime NtpEpochTime = new DateTime(1900, 1, 1);

        // time server lists
        private static string[] TimeServers = new string[]
        {
            "time.google.com",
            "time.windows.com",
            "pool.ntp.org"
        };

        // active time server
        public static string ActiveTimeServer;

        // time offset
        public static TimeSpan TimeDiff;

        // time sync round trip time
        public static TimeSpan RoundTripTime;

        // timeout 200ms
        public static int TimeOut = 200;


        public static DateTime UtcNow => Synchronized ? DateTime.UtcNow + TimeDiff : DateTime.UtcNow;

        public static DateTime Now => Synchronized ? DateTime.Now + TimeDiff : DateTime.Now;

        // seconds from 1970-01-01 00:00:00 
        public static int UnixTime => (int)(UtcNow - EpochTime).TotalSeconds;

        // milliseconds from 1970-01-01 00:00:00.000 
        public static long UnixTimeInMs => (long)(UtcNow - EpochTime).TotalMilliseconds;

        // unix time to date time
        public static DateTime FromUnixTime(uint unixTime)
        {
            return EpochTime.AddSeconds(unixTime);
        }

        public static bool Synchronized { get; private set; } = false;

        public static bool Synchronize()
        {
            return Synchronize(TimeServers, TimeOut);
        }

        public static bool Synchronize(string[] servers, int timeout)
        {
            foreach (var server in servers)
            {
                try
                {
                    using (var ntp = new NtpClient(Dns.GetHostAddresses(server)[0]))
                    {
                        var packet = ntp.Query();
                        if (packet.RoundTripTime.TotalMilliseconds < timeout)
                        {
                            TimeDiff = packet.Offset;
                            RoundTripTime = packet.RoundTripTime;

                            ActiveTimeServer = server;

                            Synchronized = true;
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Warning("exception! e.Message=", e.Message);
                }
            }

            return false;
        }
    }
}
