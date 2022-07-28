namespace Networking.Messages
{
    public sealed class UserConnectionRequestMessage : BaseMessage
    {
        public UserConnectionRequestMessage(string id)
        {
            Type = NetworkMessageType.UserConnectionRequest;
            ID = id;
        }

        public string ID { get; }
    }
}
