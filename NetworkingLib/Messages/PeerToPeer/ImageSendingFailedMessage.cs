namespace NetworkingLib.Messages
{
    public sealed class ImageSendingFailedMessage : BaseMessage
    {
        public ImageSendingFailedMessage(string messageID)
        {
            Type = NetworkMessageType.ImageSendingFailed;
            MessageID = messageID;
        }

        public string MessageID { get; set; }
    }
}
