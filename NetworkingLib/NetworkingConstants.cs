namespace NetworkingLib
{
    public static class NetworkingConstants
    {
        public static byte MaxChannelsCount { get; } = 64;
        public static byte ChannelsCount { get; } = 3;
        public static int DisconnectionTimeoutMilliseconds { get; } = 15000;
        public static int SpeedTimerFrequency { get; } = 100;
        public static string XorLayerPassword { get; } = "VerySecretSymmetricXorPassword3923";
    }
}
