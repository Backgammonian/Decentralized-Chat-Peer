namespace Networking.Messages
{
    public class KeepAliveMessage : BaseMessage
    {
        public KeepAliveMessage()
        {
            Type = NetworkMessageType.KeepAlive;
        }
    }
}
