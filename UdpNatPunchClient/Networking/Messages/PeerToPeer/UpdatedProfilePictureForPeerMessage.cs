namespace Networking.Messages
{
    public sealed class UpdatedProfilePictureForPeerMessage : BaseMessage
    {
        public UpdatedProfilePictureForPeerMessage(byte[] pictureArray, string pictureExtension)
        {
            Type = NetworkMessageType.UpdatedProfilePictureForPeer;
            UpdatedPictureArray = pictureArray;
            UpdatedPictureExtension = pictureExtension;
        }

        public byte[] UpdatedPictureArray { get; }
        public string UpdatedPictureExtension { get; }
    }
}
