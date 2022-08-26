namespace Networking.Messages
{
    public sealed class ImageMessageToPeer : BaseMessage
    {
        public ImageMessageToPeer(string authorID, string messageID, byte[] pictureBytes, string pictureExtension)
        {
            Type = NetworkMessageType.ImageMessage;
            AuthorID = authorID;
            MessageID = messageID;
            PictureBytes = pictureBytes;
            PictureExtension = pictureExtension;
        }

        public string AuthorID { get; }
        public string MessageID { get; }
        public byte[] PictureBytes { get; }
        public string PictureExtension { get; }
    }
}
