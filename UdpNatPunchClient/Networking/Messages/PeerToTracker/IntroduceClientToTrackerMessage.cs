namespace Networking.Messages
{
    public sealed class IntroduceClientToTrackerMessage : BaseMessage
    {
        public IntroduceClientToTrackerMessage(string id)
        {
            Type = NetworkMessageType.IntroduceClientToTracker;
            ID = id;
        }

        public string ID { get; }
    }
}
