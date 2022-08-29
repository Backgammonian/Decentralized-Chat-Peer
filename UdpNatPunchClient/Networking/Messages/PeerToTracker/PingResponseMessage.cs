namespace Networking.Messages
{
    public sealed class PingResponseMessage : BaseMessage
    {
        public PingResponseMessage(int ping)
        {
            Type = NetworkMessageType.PingResponse;
            Ping = ping;
        }

        public int Ping { get; set; }
    }
}
