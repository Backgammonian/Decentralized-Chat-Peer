namespace Networking.Messages
{
    public sealed class PingResponseMessage : BaseMessage
    {
        public PingResponseMessage(int ping, long packetLossPercent)
        {
            Type = NetworkMessageType.PingResponse;
            Ping = ping;
            PacketLossPercent = packetLossPercent;
        }

        public int Ping { get; }
        public long PacketLossPercent { get; }
    }
}
