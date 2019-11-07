using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Utils.Ntp
{
    public class NtpPacket
    {
        public const int Length = 48;

        public byte[] Bytes { get; private set; }

        public enum NtpMode
        {
            Client = 3,
            Server = 4,
        }

        public enum NtpLeapIndicator
        {
            NoWarning,
            LastMinuteHas61Seconds,
            LastMinuteHas59Seconds,
            AlarmCondition
        }

        public NtpLeapIndicator LeapIndicator
        {
            get { return (NtpLeapIndicator)((Bytes[0] & 0xc0) >> 6); }
        }


        public int Version
        {
            get { return (Bytes[0] & 0x38) >> 3; }
            set { Bytes[0] = (byte)((Bytes[0] & ~0x38) | value << 3); }
        }

        public NtpMode Mode
        {
            get { return (NtpMode)(Bytes[0] & 0x07); }
            set { Bytes[0] = (byte)((Bytes[0] & ~0x07) | (byte)value); }
        }

        public int Stratum => Bytes[1];

        public int Poll => Bytes[2];

        public int Precision => (sbyte)Bytes[3];

        public TimeSpan RootDelay => GetTimeSpan32(4);

        public TimeSpan RootDispersion => GetTimeSpan32(8);

        public uint ReferenceId => GetUInt32BE(12);

        public DateTime? ReferenceTimestamp
        {
            get { return GetDateTime64(16); }
            set { SetDateTime64(16, value); }
        }

        public DateTime? OriginTimestamp
        {
            get { return GetDateTime64(24); }
            set { SetDateTime64(24, value); }
        }

        public DateTime? ReceiveTimestamp
        {
            get { return GetDateTime64(32); }
            set { SetDateTime64(32, value); }
        }

        public DateTime? TransmitTimestamp
        {
            get { return GetDateTime64(40); }
            set { SetDateTime64(40, value); }
        }

        public DateTime? DestinationTimestamp { get; set; }

        public TimeSpan RoundTripTime
        {
            get
            {
                return (ReceiveTimestamp.Value - OriginTimestamp.Value) + (DestinationTimestamp.Value - TransmitTimestamp.Value);
            }
        }

        public TimeSpan Offset
        {
            get
            {
                return TimeSpan.FromTicks(((ReceiveTimestamp.Value - OriginTimestamp.Value) - (DestinationTimestamp.Value - TransmitTimestamp.Value)).Ticks / 2);
            }
        }

        public NtpPacket() : this(new byte[Length])
        {
            Mode = NtpMode.Client;
            Version = 4;
            TransmitTimestamp = DateTime.UtcNow;
        }

        public NtpPacket(byte[] bytes)
        {
            if (bytes.Length < Length) throw new ArgumentException($"NTP Packet must be at least {Length} bytes long");
            Bytes = bytes;
        }

        DateTime? GetDateTime64(int offset)
        {
            var field = GetUInt64BE(offset);
            if (field == 0) return null;
            return new DateTime(NetTime.NtpEpochTime.Ticks + Convert.ToInt64(field * (1.0 / (1L << 32) * 10000000.0)));
        }

        void SetDateTime64(int offset, DateTime? value) { SetUInt64BE(offset, value == null ? 0 : Convert.ToUInt64((value.Value.Ticks - NetTime.NtpEpochTime.Ticks) * (0.0000001 * (1L << 32)))); }
        TimeSpan GetTimeSpan32(int offset) { return TimeSpan.FromSeconds(GetInt32BE(offset) / (double)(1 << 16)); }
        ulong GetUInt64BE(int offset) { return SwapEndianness(BitConverter.ToUInt64(Bytes, offset)); }
        void SetUInt64BE(int offset, ulong value) { Array.Copy(BitConverter.GetBytes(SwapEndianness(value)), 0, Bytes, offset, 8); }
        int GetInt32BE(int offset) { return (int)GetUInt32BE(offset); }
        uint GetUInt32BE(int offset) { return SwapEndianness(BitConverter.ToUInt32(Bytes, offset)); }
        static uint SwapEndianness(uint x) { return ((x & 0xff) << 24) | ((x & 0xff00) << 8) | ((x & 0xff0000) >> 8) | ((x & 0xff000000) >> 24); }
        static ulong SwapEndianness(ulong x) { return ((ulong)SwapEndianness((uint)x) << 32) | SwapEndianness((uint)(x >> 32)); }
    }
}
