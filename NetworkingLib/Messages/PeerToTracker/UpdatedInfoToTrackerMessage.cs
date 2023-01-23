namespace NetworkingLib.Messages
{
    public sealed class UpdatedInfoToTrackerMessage : BaseMessage
    {
        public UpdatedInfoToTrackerMessage(string newNickname)
        {
            Type = NetworkMessageType.UpdatedInfoForTracker;
            NewNickname = newNickname;
        }

        public string NewNickname { get; set; }
    }
}
