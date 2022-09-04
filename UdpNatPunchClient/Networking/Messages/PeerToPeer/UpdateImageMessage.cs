namespace Networking.Messages
{
    public sealed class UpdateImageMessage : BaseMessage
    {
        public UpdateImageMessage(string messageID, byte[] pictureBytes, string pictureExtension)
        {
            Type = NetworkMessageType.UpdateImageMessage;
            MessageID = messageID;
            PictureBytes = pictureBytes;
            PictureExtension = pictureExtension;
        }

        public string MessageID { get; set; }
        public byte[] PictureBytes { get; set; }
        public string PictureExtension { get; set; }
    }
}
