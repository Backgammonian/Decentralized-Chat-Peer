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

        public string AuthorID { get; set; }
        public string MessageID { get; set; }
        public byte[] PictureBytes { get; set; }
        public string PictureExtension { get; set; }
    }
}
