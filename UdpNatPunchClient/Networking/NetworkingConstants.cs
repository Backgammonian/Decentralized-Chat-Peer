namespace Networking
{
    public static class NetworkingConstants
    {
        public static int FileSegmentSize { get; } = 32768;
        public static byte MaxChannelsCount { get; } = 64;
        public static byte ChannelsCount { get; } = MaxChannelsCount;
        public static int DisconnectionTimeoutMilliseconds { get; } = 15000;
        public static int SpeedTimerFrequency { get; } = 10;
    }
}
