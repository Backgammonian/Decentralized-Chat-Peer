namespace Networking.Messages
{
    public sealed class UpdatedInfoForPeerMessage : BaseMessage
    {
        public UpdatedInfoForPeerMessage(string nickname)
        {
            Type = NetworkMessageType.UpdatedInfoForPeer;
            UpdatedNickname = nickname;
        }

        public string UpdatedNickname { get; set; }
    }
}
