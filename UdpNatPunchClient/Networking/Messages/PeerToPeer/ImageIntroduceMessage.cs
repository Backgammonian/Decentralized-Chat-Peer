namespace Networking.Messages
{
    public sealed class ImageIntroduceMessage : BaseMessage
    {
        public ImageIntroduceMessage(string messageID)
        {
            Type = NetworkMessageType.ImageIntroduceMessage;
            MessageID = messageID;
        }

        public string MessageID { get; set; }
    }
}
