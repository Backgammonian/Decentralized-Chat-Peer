namespace NetworkingLib.Messages
{
    public sealed class MessageReadNotification : BaseMessage
    {
        public MessageReadNotification(string messageID)
        {
            Type = NetworkMessageType.MessageReadNotification;
            MessageID = messageID;
        }

        public string MessageID { get; set; }
    }
}
