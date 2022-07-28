namespace Networking.Messages
{
    public sealed class ForwardedConnectionRequestMessage : BaseMessage
    {
        public ForwardedConnectionRequestMessage(string id, string endPointString)
        {
            Type = NetworkMessageType.ForwardedConnectionRequest;
            ID = id;
            EndPointString = endPointString;
        }

        public string ID { get; }
        public string EndPointString { get; }
    }
}