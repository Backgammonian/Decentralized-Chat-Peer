namespace Networking.Messages
{
    public sealed class UserConnectionResponseMessage : BaseMessage
    {
        public UserConnectionResponseMessage(string id, string endPointString)
        {
            Type = NetworkMessageType.UserConnectionResponse;
            ID = id;
            EndPointString = endPointString;
        }

        public string ID { get; }
        public string EndPointString { get; }
    }
}