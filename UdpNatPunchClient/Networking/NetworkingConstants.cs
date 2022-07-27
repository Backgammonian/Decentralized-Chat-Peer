using System;

namespace Networking
{
    public static class NetworkingConstants
    {
        public static int FileSegmentSize { get; } = Convert.ToInt32(Math.Pow(2, 18));
        public static byte ChannelsCount { get; } = 8;
        public static int DisconnectionTimeoutMilliseconds { get; } = 15000;
    }
}
