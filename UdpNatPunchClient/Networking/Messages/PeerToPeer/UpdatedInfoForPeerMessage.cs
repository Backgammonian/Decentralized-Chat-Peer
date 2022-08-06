namespace Networking.Messages
{
    public sealed class UpdatedInfoForPeerMessage : BaseMessage
    {
        public UpdatedInfoForPeerMessage(string nickname)
        {
            UpdatedNickname = nickname;
        }

        public string UpdatedNickname { get; }
    }
}
