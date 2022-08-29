namespace Networking.Messages
{
    public class MessageReadNotification : BaseMessage
    {
        public MessageReadNotification(string messageID)
        {
            Type = NetworkMessageType.MessageReadNotification;
            MessageID = messageID;
        }

        public string MessageID { get; set; }
    }
}
