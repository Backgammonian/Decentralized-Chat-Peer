namespace Networking.Messages
{
    public sealed class KeepAliveMessage : BaseMessage
    {
        public KeepAliveMessage()
        {
            Type = NetworkMessageType.KeepAlive;
        }
    }
}
