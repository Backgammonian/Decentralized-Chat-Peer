namespace Networking.Messages
{
    public sealed class UpdatedProfilePictureForPeerMessage : BaseMessage
    {
        public UpdatedProfilePictureForPeerMessage(string pictureBase64)
        {
            UpdatedPictureBase64 = pictureBase64;
        }

        public string UpdatedPictureBase64 { get; }
    }
}
