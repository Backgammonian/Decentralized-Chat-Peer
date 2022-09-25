namespace Networking
{
    public static class NetworkingConstants
    {
        public static byte MaxChannelsCount { get; } = 64;
        //1st channel - for all messages (except the cases below)
        //2nd channel - for image messages
        //3rd channel - for file segments
        public static byte ChannelsCount { get; } = 3;
        public static int DisconnectionTimeoutMilliseconds { get; } = 15000;
        public static int SpeedTimerFrequency { get; } = 100;
    }
}
