namespace Networking.Messages
{
    public sealed class UpdatedProfilePictureForPeerMessage : BaseMessage
    {
        public UpdatedProfilePictureForPeerMessage(byte[] pictureArray, string pictureExtension)
        {
            UpdatedPictureArray = pictureArray;
            UpdatedPictureExtension = pictureExtension;
        }

        public byte[] UpdatedPictureArray { get; }
        public string UpdatedPictureExtension { get; }
    }
}
