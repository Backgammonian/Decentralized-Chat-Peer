namespace Networking.Messages
{
    public sealed class MessageReceiptNotification : BaseMessage
    {
        public MessageReceiptNotification(string messageID)
        {
            Type = NetworkMessageType.MessageReceiptNotification;
            MessageID = messageID;
        }

        public string MessageID { get; }
    }
}
