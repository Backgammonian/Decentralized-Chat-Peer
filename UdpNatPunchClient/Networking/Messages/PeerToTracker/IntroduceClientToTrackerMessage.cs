namespace Networking.Messages
{
    public sealed class IntroduceClientToTrackerMessage : BaseMessage
    {
        public IntroduceClientToTrackerMessage(string id, string nickname)
        {
            Type = NetworkMessageType.IntroduceClientToTracker;
            ID = id;
            Nickname = nickname;
        }

        public string ID { get; set; }
        public string Nickname { get; set; }
    }
}
